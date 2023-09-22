package com.github.k1rakishou.fsaf

import android.content.Context
import android.net.Uri
import android.os.Environment
import android.os.ParcelFileDescriptor
import android.provider.DocumentsContract
import android.util.Log
import android.annotation.SuppressLint
import androidx.documentfile.provider.DocumentFile
import com.github.k1rakishou.fsaf.document_file.CachingDocumentFile
import com.github.k1rakishou.fsaf.extensions.copyInto
import com.github.k1rakishou.fsaf.extensions.extension
import com.github.k1rakishou.fsaf.extensions.splitIntoSegments
import com.github.k1rakishou.fsaf.file.*
import com.github.k1rakishou.fsaf.manager.BaseFileManager
import com.github.k1rakishou.fsaf.manager.ExternalFileManager
import com.github.k1rakishou.fsaf.manager.RawFileManager
import com.github.k1rakishou.fsaf.manager.SnapshotFileManager
import com.github.k1rakishou.fsaf.manager.base_directory.BaseDirectory
import com.github.k1rakishou.fsaf.manager.base_directory.DirectoryManager
import com.github.k1rakishou.fsaf.util.FSAFUtils
import com.github.k1rakishou.fsaf.util.SAFHelper
import java.io.*
import java.util.*


class FileManager(
  private val appContext: Context,
  private val badPathSymbolResolutionStrategy: BadPathSymbolResolutionStrategy =
    BadPathSymbolResolutionStrategy.ReplaceBadSymbols,
  private val directoryManager: DirectoryManager = DirectoryManager(appContext)
) : BaseFileManager {
  private val managers = mutableMapOf<FileManagerId, BaseFileManager>()
  private val externalFileManager: BaseFileManager = ExternalFileManager(
    appContext,
    badPathSymbolResolutionStrategy,
    directoryManager
  )
  private val rawFileManager: BaseFileManager = RawFileManager(
    badPathSymbolResolutionStrategy
  )

  init {
    addCustomFileManager(ExternalFile.FILE_MANAGER_ID, externalFileManager)
    addCustomFileManager(RawFile.FILE_MANAGER_ID, rawFileManager)
  }

  /**
   * Add your own custom file managers (like, maybe, TestFileManager or something like that?)
   * */
  @Synchronized
  fun addCustomFileManager(fileManagerId: FileManagerId, customManager: BaseFileManager) {
    if (managers.containsKey(fileManagerId)) {
      Log.e(TAG, "FileManager with id ${fileManagerId} has already been added!")
      return
    }

    managers[fileManagerId] = customManager
  }

  /**
   * It's not a good idea to use this method to remove standard file managers (externalFileManager
   * and rawFileManager) but you should use it to remove your own file managers.
   * */
  @Synchronized
  fun removeCustomFileManager(fileManagerId: FileManagerId) {
    managers.remove(fileManagerId)
  }

  @Synchronized
  fun getExternalFileManager(): ExternalFileManager {
    return managers[ExternalFile.FILE_MANAGER_ID] as? ExternalFileManager
      ?: throw IllegalStateException("ExternalFileManager is not added")
  }

  @Synchronized
  fun getRawFileManager(): RawFileManager {
    return managers[RawFile.FILE_MANAGER_ID] as? RawFileManager
      ?: throw IllegalStateException("RawFileManager is not added")
  }

  /**
   * Creates a raw file from a path.
   * Use this method to convert a java File by this path into an AbstractFile.
   * Does not create file on the disk automatically!
   * */
  @Synchronized
  fun fromPath(path: String): RawFile {
    return fromRawFile(File(path))
  }

  /**
   * Creates an external file from Uri.
   * Use this method to convert external file uri (file that may be located at sd card) into an
   * AbstractFile. If a file does not exist null is returned.
   * Does not create file on the disk automatically!
   * */
  @Synchronized
  fun fromUri(uri: Uri): ExternalFile? {
    val documentFile = toDocumentFile(uri)
      ?: return null

    return if (documentFile.isFile()) {
      val filename = documentFile.name()
        ?: return null

      ExternalFile(
        appContext,
        badPathSymbolResolutionStrategy,
        Root.FileRoot(documentFile, filename)
      )
    } else {
      ExternalFile(
        appContext,
        badPathSymbolResolutionStrategy,
        Root.DirRoot(documentFile)
      )
    }
  }

  /**
   * Creates RawFile from the Java File.
   * Use this method to convert a java File into an AbstractFile.
   * Does not create file on the disk automatically!
   * */
  @Synchronized
  fun fromRawFile(file: File): RawFile {
    if (file.isFile) {
      return RawFile(
        Root.FileRoot(file, file.name),
        badPathSymbolResolutionStrategy
      )
    }

    return RawFile(
      Root.DirRoot(file),
      badPathSymbolResolutionStrategy
    )
  }

  inline fun <reified T> registerBaseDir(baseDirectory: BaseDirectory) {
    registerBaseDir(T::class.java, baseDirectory)
  }

  @Synchronized
  fun registerBaseDir(clazz: Class<*>, baseDirectory: BaseDirectory) {
    directoryManager.registerBaseDir(clazz, baseDirectory)
  }

  inline fun <reified T> unregisterBaseDir() {
    unregisterBaseDir(T::class.java)
  }

  @Synchronized
  fun unregisterBaseDir(clazz: Class<*>) {
    directoryManager.unregisterBaseDir(clazz)
  }

  inline fun <reified T> newBaseDirectoryFile(): AbstractFile? {
    return newBaseDirectoryFile(T::class.java)
  }

  inline fun <reified T> isBaseDirAlreadyRegistered(file: AbstractFile): Boolean {
    return isBaseDirAlreadyRegistered(T::class.java, file)
  }

  /**
   * [clazz] is a class of base directory we want to to update
   * */
  @Synchronized
  fun isBaseDirAlreadyRegistered(clazz: Class<*>, file: AbstractFile): Boolean {
    return directoryManager.isAlreadyRegistered(clazz, file)
  }

  inline fun <reified T> isBaseDirAlreadyRegistered(filePath: String): Boolean {
    return isBaseDirAlreadyRegistered(T::class.java, filePath)
  }

  /**
   * [clazz] is a class of base directory we want to to update
   * */
  @Synchronized
  fun isBaseDirAlreadyRegistered(clazz: Class<*>, filePath: String): Boolean {
    return directoryManager.isAlreadyRegistered(clazz, filePath)
  }

  inline fun <reified T> isBaseDirAlreadyRegistered(uri: Uri): Boolean {
    return isBaseDirAlreadyRegistered(T::class.java, uri)
  }

  /**
   * [clazz] is a class of base directory we want to to update
   * */
  @Synchronized
  fun isBaseDirAlreadyRegistered(clazz: Class<*>, uri: Uri): Boolean {
    return directoryManager.isAlreadyRegistered(clazz, uri)
  }

  /**
   * Instantiates a new AbstractFile with the root being in the base directory with [clazz]
   * base directory id.
   *
   * Does not create file on the disk automatically! (just like the Java File)
   * Use one of the [create] methods to create a file on the disk.
   * */
  @Synchronized
  fun newBaseDirectoryFile(clazz: Class<*>): AbstractFile? {
    val baseDir = directoryManager.getBaseDirByClass(clazz)
    if (baseDir == null) {
      Log.e(TAG, "Base directory with class $clazz is not registered")
      return null
    }

    val resultFile = when (baseDir.currentActiveBaseDirType()) {
      BaseDirectory.ActiveBaseDirType.SafBaseDir -> {
        val dirUri = checkNotNull(baseDir.getDirUri()) {
          "Current active base dir is SafBaseDir but baseDir.getDirUri() returned null! " +
            "If a base dir is active it must be not null!"
        }

        if (!SAFHelper.isTreeUri(baseDir)) {
          Log.e(TAG, "Not a tree uri ${baseDir.dirPath()}")
          return null
        }

        if (!directoryManager.checkBaseDirSafStillHasPermissions(dirUri)) {
          Log.e(TAG, "No permissions for base directory ${dirUri}")
          return null
        }

        val treeDirectory = try {
          DocumentFile.fromTreeUri(appContext, dirUri)
        } catch (error: Throwable) {
          Log.e(TAG, "Error while trying to create TreeDocumentFile, dirUri = ${baseDir.dirPath()}")
          return null
        }

        if (treeDirectory == null) {
          return null
        }

        ExternalFile(
          appContext,
          badPathSymbolResolutionStrategy,
          Root.DirRoot(CachingDocumentFile(appContext, treeDirectory))
        )
      }
      BaseDirectory.ActiveBaseDirType.JavaFileBaseDir -> {
        val dirFile = checkNotNull(baseDir.getDirFile()) {
          "Current active base dir is JavaFileBaseDir but baseDir.getDirFile() returned null! " +
            "If a base dir is active it must be not null!"
        }

        RawFile(
          Root.DirRoot(dirFile),
          badPathSymbolResolutionStrategy
        )
      }
      else -> {
        throw IllegalStateException("${baseDir.javaClass.name} is not supported!")
      }
    }

    if (!exists(resultFile)) {
      Log.e(TAG, "Base directory ${clazz} with path ${resultFile.getFullPath()} does not exist!")
      return null
    }

    if (!isDirectory(resultFile)) {
      Log.e(TAG, "Base directory ${clazz} with path ${resultFile.getFullPath()} is not a directory!")
      return null
    }

    if (resultFile is ExternalFile) {
      if (resultFile.getFileRoot<CachingDocumentFile>().holder.isVirtual()) {
        Log.e(TAG, "Base directory ${clazz} with path ${resultFile.getFullPath()} is a virtual file!")
        return null
      }
    }

    return resultFile
  }

  inline fun <reified T> baseDirectoryExists(): Boolean {
    return baseDirectoryExists(T::class.java)
  }

  /**
   * Checks whether at least one of the directory type exists for the current [BaseDirectory].
   * [BaseDirectory] may have a SAF directory and a fallback java file backed directory. If neither
   * of them is registered or they both return null this method will return null as well.
   * */
  @Synchronized
  @Suppress("UNCHECKED_CAST")
  fun baseDirectoryExists(clazz: Class<*>): Boolean {
    val baseDirFile = newBaseDirectoryFile(clazz as Class<BaseDirectory>)
    if (baseDirFile == null) {
      Log.e(TAG, "baseDirectoryExists() newBaseDirectoryFile returned null")
      return false
    }

    if (!exists(baseDirFile)) {
      return false
    }

    return true
  }

  /**
   * Similar to [AbstractFile.cloneUnsafe]. Use only if you are sure in your input.
   * */
  @Synchronized
  fun createUnsafe(baseDir: AbstractFile, path: String): AbstractFile? {
    val segmentStrings = path.splitIntoSegments()

    val segments = segmentStrings.mapIndexed { index, segmentString ->
      if (index == segmentStrings.lastIndex && segmentString.extension() != null) {
        return@mapIndexed FileSegment(segmentString)
      }

      return@mapIndexed DirectorySegment(segmentString)
    }

    return create(baseDir, segments)
  }

  /**
   * [name] should not contain an extension
   * */
  fun createDir(baseDir: AbstractFile, name: String): AbstractFile? {
    return create(baseDir, DirectorySegment(name))
  }

  /**
   * [name] may contain an extension
   * */
  fun createFile(baseDir: AbstractFile, name: String): AbstractFile? {
    return create(baseDir, FileSegment(name))
  }

  fun create(baseDir: AbstractFile, segment: Segment): AbstractFile? {
    return create(baseDir, listOf(segment))
  }

  override fun create(baseDir: AbstractFile): AbstractFile? {
    return create(baseDir, baseDir.getFileSegments())
  }

  fun create(baseDir: AbstractFile, vararg segments: Segment): AbstractFile? {
    return create(baseDir, segments.toList())
  }

  @Synchronized
  override fun create(baseDir: AbstractFile, segments: List<Segment>): AbstractFile? {
    if (segments.isEmpty() && exists(baseDir)) {
      return baseDir
    }

    val segmentsToAppend = if (segments.isEmpty()) {
      if (baseDir.getFileSegments().isEmpty()) {
        emptyList()
      } else {
        baseDir.getFileSegments()
      }
    } else {
      segments
    }

    val manager = managers[baseDir.getFileManagerId()]
      ?: throw NotImplementedError(
        "Not implemented for ${baseDir.javaClass.name}, " +
          "fileManagerId = ${baseDir.getFileManagerId()}"
      )

    return manager.create(
      baseDir.clone(),
      FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(
        badPathSymbolResolutionStrategy,
        segmentsToAppend
      )
    )
  }

  /**
   * Copies one file's contents into another. Does not, in any way, modify the source file
   * */
  @Synchronized
  fun copyFileContents(sourceFile: AbstractFile, destinationFile: AbstractFile): Boolean {
    return try {
      getInputStream(sourceFile)?.use { inputStream ->
        getOutputStream(destinationFile)?.use { outputStream ->
          inputStream.copyInto(outputStream)
          return@use true
        }
      } ?: false
    } catch (e: IOException) {
      Log.e(TAG, "IOException while copying one file into another", e)
      false
    }
  }

  /**
   * [recursively] will copy [sourceDir]'s sub directories and files as well, if false then only the
   * contents of the [sourceDir] will be copied into the [destDir].
   *
   * [updateFunc] is a callback that has two parameters:
   * 1 - How many files have been copied
   * 2 - Total amount of files to be copied
   *
   * if [updateFunc] returns true that means that we need to cancel the copying. If [updateFunc] is
   * not provided then there is no way to cancel this method
   * */
  @Synchronized
  fun copyDirectoryWithContent(
    sourceDir: AbstractFile,
    destDir: AbstractFile,
    recursively: Boolean,
    // You may want to show some kind of a progress dialog which will show how many files have
    // been copied so far and how many are left
    updateFunc: ((Int, Int) -> Boolean)? = null
  ): Boolean {
    Log.e(TAG, "In copy contents")
    if (!exists(sourceDir)) {
      Log.e(TAG, "Source directory does not exists, path = ${sourceDir.getFullPath()}")
      return false
    }

    if (listFiles(sourceDir).isEmpty()) {
      Log.d(TAG, "Source directory is empty, nothing to copy")
      return true
    }
    Log.e(TAG, "FileManagers: ${sourceDir.getFileManagerId()} ${managers[sourceDir.getFileManagerId()]}")

    listFiles(sourceDir).forEach { innerFile ->
            Log.d(TAG, "Contains: $innerFile")
          }

    if (!exists(destDir)) {
      Log.e(TAG, "Destination directory does not exists, path = ${sourceDir.getFullPath()}")
      return false
    }

    if (!isDirectory(sourceDir)) {
      Log.e(TAG, "Source directory is not a directory, path = ${sourceDir.getFullPath()}")
      return false
    }

    if (!isDirectory(destDir)) {
      Log.e(TAG, "Destination directory is not a directory, path = ${destDir.getFullPath()}")
      return false
    }

    Log.e(TAG, "Traverse directory.")
    // TODO: instead of collecting all of the files and only then start copying, do the copying
    //  file by file inside the traverse callback to save the memory
    val files = LinkedList<AbstractFile>()
    traverseDirectory(sourceDir, recursively, TraverseMode.Both) { file ->
      files += file
    }

    if (files.isEmpty()) {
      // No files, so do nothing
      Log.d(TAG, "No files were collected, nothing to copy")
      return true
    }

    val totalFilesCount = files.size
    val prefix = sourceDir.getFullPath()
     Log.e(TAG, "Start copy.")
    for ((currentFileIndex, file) in files.withIndex()) {
      // Holy shit this hack is so fucking disgusting and may break literally any minute.
      // If this shit breaks then blame google for providing such a bad API.

      // Basically we have a directory, let's say /123 and we want to copy all
      // of it's files into /456. So we collect every file in /123 then we iterate every
      // collected file, remove base directory prefix (/123 in this case) and recreate this
      // file with the same directory structure in another base directory (/456 in this case).
      // Let's say we have the following files:
      //
      // /123/1.txt
      // /123/111/2.txt
      // /123/222/3.txt
      //
      // After calling this method we will have these files copied into /456:
      //
      // /456/1.txt
      // /456/111/2.txt
      // /456/222/3.txt
      //
      // Such is the idea of this hack.

      if (directoryManager.isBaseDir(file)) {
        // Base directory
        continue
      }

      val rawSegments = file.getFullPath()
        .removePrefix(prefix)
        .splitIntoSegments()

      if (rawSegments.isEmpty()) {
        // The sourceDir itself
        continue
      }

      // TODO: we probably can optimise this by filtering segments that are already a part of other
      //  segments, i.e. "/123/456" is a part of "/123/456/file.txt" so when we create
      //  "/123/456/file.txt" we also automatically create "/123/456" so we can filter it out
      //  beforehand thus reducing the needed amount of file operations

      val segments = if (isFile(file)) {
        // Last segment must be a file name
        rawSegments.mapIndexed { index, segmentName ->
          if (index == rawSegments.lastIndex) {
            return@mapIndexed FileSegment(segmentName)
          } else {
            return@mapIndexed DirectorySegment(segmentName)
          }
        }
      } else {
        // All segments are directory segments
        rawSegments.map { segmentName -> DirectorySegment(segmentName) }
      }

      val newFile = create(destDir, segments)
      if (newFile == null) {
        Log.e(TAG, "Couldn't create inner file with name ${getName(file)}")
        return false
      }

      if (isFile(file)) {
        if (!copyFileContents(file, newFile)) {
          Log.e(TAG, "Couldn't copy one file into another")
          return false
        }
      }

      val result = updateFunc?.invoke(currentFileIndex, totalFilesCount) ?: false
      if (result) {
        Log.e(TAG, "Cancelled")
        return true
      }
    }

    return true
  }

  /**
   * Traverses the current directory (or also any sub-directory if [recursively] is true) and passes
   * files or directories (or both) into the [func] callback. Does not pass the directory where the
   * traversing has been started.
   *
   * [recursively] means that all of the subdirectories and their subdirectories will be traversed.
   * If false then only the [directory] dir's files will be traversed. If [traverseMode]
   * is [TraverseMode.OnlyFiles] and [recursively] is true then all the sub-directories will be
   * traversed but only the files will be passed into the callback
   * */
  @Synchronized
  fun traverseDirectory(
    directory: AbstractFile,
    recursively: Boolean,
    traverseMode: TraverseMode,
    func: (AbstractFile) -> Unit
  ) {
    if (!exists(directory)) {
      Log.e(TAG, "Source directory does not exists, path = ${directory.getFullPath()}")
      return
    }

    if (!isDirectory(directory)) {
      Log.e(TAG, "Source directory is not a directory, path = ${directory.getFullPath()}")
      return
    }

    if (!recursively) {
      listFiles(directory).forEach { file -> func(file) }
      return
    }

    val queue = LinkedList<AbstractFile>()
    queue.offer(directory)

    val directoryFullPath = directory.getFullPath()

    // Collect all of the inner files in the source directory
    while (queue.isNotEmpty()) {
      val file = queue.poll()
        ?: break
      //Log.e(TAG, "${queue.size} Traversing $file fullPath: $directoryFullPath")
      when {
        isDirectory(file) -> {
          val innerFiles = listFiles(file)
          if (traverseMode.includeDirs()) {
            if (file.getFullPath() != directoryFullPath) {
              func(file)
            }
          }

          innerFiles.forEach { innerFile ->
            queue.offer(innerFile)
          }
        }
        isFile(file) -> {
          if (traverseMode.includeFiles()) {
            if (file.getFullPath() != directoryFullPath) {
              func(file)
            }
          }
        }
        else -> Log.e(TAG, "file (${file.getFullPath()}) is neither a file nor a directory " +
          "(exists = ${exists(file)}")
      }
    }
  }

  @Synchronized
  override fun exists(file: AbstractFile): Boolean {
    return managers[file.getFileManagerId()]?.exists(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun isFile(file: AbstractFile): Boolean {
    return managers[file.getFileManagerId()]?.isFile(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun isDirectory(file: AbstractFile): Boolean {
    return managers[file.getFileManagerId()]?.isDirectory(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun canRead(file: AbstractFile): Boolean {
    return managers[file.getFileManagerId()]?.canRead(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun canWrite(file: AbstractFile): Boolean {
    return managers[file.getFileManagerId()]?.canWrite(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun getSegmentNames(file: AbstractFile): List<String> {
    return managers[file.getFileManagerId()]?.getSegmentNames(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun delete(file: AbstractFile): Boolean {
    return managers[file.getFileManagerId()]?.delete(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun deleteContent(dir: AbstractFile): Boolean {
    return managers[dir.getFileManagerId()]?.deleteContent(dir)
      ?: throw NotImplementedError(
        "Not implemented for ${dir.javaClass.name}, fileManagerId = ${dir.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun getInputStream(file: AbstractFile): InputStream? {
    val manager = managers[file.getFileManagerId()]
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )

    return manager.getInputStream(file)
  }

  @Synchronized
  override fun getOutputStream(file: AbstractFile): OutputStream? {
    val manager = managers[file.getFileManagerId()]
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )

    return manager.getOutputStream(file)
  }

  @Synchronized
  override fun getName(file: AbstractFile): String {
    return managers[file.getFileManagerId()]?.getName(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun flattenSegments(file: AbstractFile): AbstractFile? {
    val manager = managers[file.getFileManagerId()]
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )

    return manager.flattenSegments(file)
  }

  @Synchronized
  override fun findFile(dir: AbstractFile, fileName: String): AbstractFile? {
    val manager = managers[dir.getFileManagerId()]
      ?: throw NotImplementedError(
        "Not implemented for ${dir.javaClass.name}, fileManagerId = ${dir.getFileManagerId()}"
      )

    return manager.findFile(dir, fileName)
  }

  @Synchronized
  override fun getLength(file: AbstractFile): Long {
    return managers[file.getFileManagerId()]?.getLength(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun listFiles(dir: AbstractFile): List<AbstractFile> {
    return managers[dir.getFileManagerId()]?.listFiles(dir)
      ?: throw NotImplementedError(
        "Not implemented for ${dir.javaClass.name}, fileManagerId = ${dir.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun lastModified(file: AbstractFile): Long {
    return managers[file.getFileManagerId()]?.lastModified(file)
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, " +
          "fileManagerId = ${file.getFileManagerId()}"
      )
  }

  @Synchronized
  override fun getParcelFileDescriptor(
    file: AbstractFile,
    fileDescriptorMode: FileDescriptorMode
  ): ParcelFileDescriptor? {
    val manager = managers[file.getFileManagerId()]
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )

    return manager.getParcelFileDescriptor(file, fileDescriptorMode)
  }

  @Synchronized
  override fun <T> withFileDescriptor(
    file: AbstractFile,
    fileDescriptorMode: FileDescriptorMode,
    func: (FileDescriptor) -> T?
  ): T? {
    val manager = managers[file.getFileManagerId()]
      ?: throw NotImplementedError(
        "Not implemented for ${file.javaClass.name}, fileManagerId = ${file.getFileManagerId()}"
      )

    return manager.withFileDescriptor(file, fileDescriptorMode, func)
  }


  /**
   * Manually creates a snapshot of a directory (with sub directories if [includeSubDirs] is true).
   * A SnapshotFileManager will be returned which uses the same interface as the other FileManagers.
   *
   * Snapshot is only created for ExternalFiles! RawFiles are not supported and snapshot won't be
   * created (RawFileManager will be returned instead). This method exists to make bulk
   * Storage Access Framework operations faster!
   *
   * @note: In case of RawFile the Java File API will be used (instead of creating a snapshot) which
   * is pretty fast by itself. So no snapshot will be created in case of RawFile.
   * */
  @Synchronized
  fun createSnapshot(dir: AbstractFile, includeSubDirs: Boolean = false): BaseFileManager {
    require(isDirectory(dir)) { "dir is not a directory" }

    if (dir !is ExternalFile) {
      return rawFileManager
    }

    val fastFileSearchTree = FastFileSearchTree<CachingDocumentFile>()

    // Insert the dir too otherwise we won't be able to do anything with it's children
    insertAbstractFileIntoTree(dir, fastFileSearchTree)

    // We only travers directories here because to get the  files we use SAFHelper.listFilesFast
    // which is specifically designed to be used with Snapshots
    traverseDirectory(dir, includeSubDirs, TraverseMode.OnlyDirs) { directory ->
      insertAbstractFileIntoTree(directory, fastFileSearchTree)
    }

    return SnapshotFileManager(
      appContext,
      badPathSymbolResolutionStrategy,
      directoryManager,
      fastFileSearchTree
    )
  }

  private fun insertAbstractFileIntoTree(
    abstractFile: AbstractFile,
    fastFileSearchTree: FastFileSearchTree<CachingDocumentFile>
  ) {
    val parentUri = abstractFile.getFileRoot<CachingDocumentFile>().holder.uri()
    val isBaseDir = directoryManager.isBaseDir(abstractFile)

    val documentFiles = SAFHelper.listFilesFast(appContext, parentUri, isBaseDir)
    documentFiles.forEach { documentFile ->
      val segments = documentFile.uri().toString().splitIntoSegments()

      check(fastFileSearchTree.insertSegments(segments, documentFile)) {
        "createSnapshot() Couldn't insert new segments (${segments}) " +
          "into the tree for file (${documentFile.uri()})"
      }
    }
  }

  /**
   * Returns true if a [file] is somewhere inside the [dir] directory. It may not be exactly inside
   * the [dir] but somewhere deeper (e.g. [dir] = /test, [file] = /test/1/2/3/4/file.txt will
   * return true).
   * Also returns false is both [dir] and [file] are directories with the same path
   * (basically if areTheSame(dir, file) == true)
   *
   * [dir] must be a directory! Will throw an IllegalArgumentException is a file is passed.
   * [file] may be a file or a directory.
   *
   * Does not check whether either of the input files exist!
   * */
  @Synchronized
  fun isChildOfDirectory(dir: AbstractFile, file: AbstractFile): Boolean {
    require(isDirectory(dir)) { "dir must be a directory!" }

    if (areTheSame(dir, file)) {
      return false
    }

    if (dir is RawFile && file is RawFile) {
      val dirFullPath = dir.getFullPath()
      val fileFullPath = file.getFullPath()

      val dirSegments = dirFullPath.splitIntoSegments()
      val fileSegments = fileFullPath.splitIntoSegments()

      return compareSegments(dirFullPath, dirSegments, fileFullPath, fileSegments)
    } else {

      val (dirSegmentsPath, dirStorageId) = extractSegmentsPathAndStorageId(dir)
      val (fileSegmentsPath, fileStorageId) = extractSegmentsPathAndStorageId(file)

      if (dirStorageId != fileStorageId) {
        return false
      }

      val dirSegments = dirSegmentsPath.splitIntoSegments()
      val fileSegments = fileSegmentsPath.splitIntoSegments()

      return compareSegments(dirSegmentsPath, dirSegments, fileSegmentsPath, fileSegments)
    }
  }

  private fun compareSegments(
    dirFullPath: String,
    dirSegments: List<String>,
    fileFullPath: String,
    fileSegments: List<String>
  ): Boolean {
    when {
      // false
      // dir = "Kuroba-dev/files/files2"
      // file = "Kuroba-dev/test123.txt"
      fileSegments.size < dirSegments.size -> return false

      // false
      // dir = "Kuroba-dev/files"
      // file = "Kuroba-dev/files_new"

      // false
      // dir = "Kuroba-dev/files/files2"
      // file = "Kuroba-dev/files/test123.txt"

      // dir = "Kuroba-dev/files"
      // file = "Kuroba-dev/files"
      fileSegments.size == dirSegments.size -> return false

      // true
      // dir = "/storage/emulated/0/Kuroba-dev/files/files2"
      // file = "/storage/emulated/0/Kuroba-dev/files/files2/test123.txt"
      else -> return fileFullPath.startsWith(dirFullPath)
    }
  }

  /**
   * A handy method that returns true if the two [AbstractFile]s point to the same file or directory
   * even when they are backed by the different file API (e.g. file1 is a RawFile and file2 is an
   * ExternalFile).
   *
   * Does not check whether either of the input files exist!
   * */
  @Synchronized
  fun areTheSame(file1: AbstractFile, file2: AbstractFile): Boolean {
    if (getName(file1) != getName(file2)) {
      // Names differ
      return false
    }

    if (file1 is RawFile && file2 is RawFile) {
      return file1.getFullPath() == file2.getFullPath()
    }

    if (file1 is ExternalFile && file2 is ExternalFile) {
      return file1.getFullPath() == file2.getFullPath()
    }

    val (segmentsPath1, storageId1) = extractSegmentsPathAndStorageId(file1)
    val (segmentsPath2, storageId2) = extractSegmentsPathAndStorageId(file2)

    return storageId1 == storageId2 && segmentsPath1 == segmentsPath2
  }

  private fun extractSegmentsPathAndStorageId(
    file: AbstractFile
  ): Pair<String, String> {

    /**
     * !!! Experimental !!!
     * */
    // FIXME: this won't work on Android 10
    // TODO: what if a file is located in the internal storage? (appContext.filesDir)
    val externalStoragePath = Environment.getExternalStorageDirectory().absolutePath

    val segmentsPath = when (file) {
      is RawFile -> file.getFullPath()
        .removePrefix(externalStoragePath)
        .splitIntoSegments()
        .joinToString(separator = "/")
      is ExternalFile -> splitDocumentId(file) { documentId ->
        val indexOfColon = documentId.indexOf(':')
        return@splitDocumentId documentId.substring(indexOfColon + 1)
      }
      else -> throw NotImplementedError("Not implemented for ${file::class.java}")
    }

    val storageId1 = splitDocumentId(file) { documentId ->
      val indexOfColon = documentId.indexOf(':')
      return@splitDocumentId documentId.substring(0, indexOfColon)
    }

    return Pair(segmentsPath, storageId1)
  }

  /**
   * !!! Experimental !!!
   * */
  @SuppressLint("NewApi")
  private fun splitDocumentId(file: AbstractFile, spliterator: (String) -> String): String {
    if (file is RawFile) {
      // RawFiles have primary storage since they can't access sd-card
      return PRIMARY_STORAGE
    }

    val uri = Uri.parse(file.getFullPath())

    // This will returns us a string like "primary:dir1/dir2 .. etc", we need to remove the
    // "primary:" prefix
    val documentId = DocumentsContract.getDocumentId(uri)

    val colonsCount = documentId.count { ch -> ch == ':' }
    if (colonsCount == 0) {
      return documentId.splitIntoSegments().joinToString(separator = "/")
    }

    if (colonsCount > 1) {
      Log.e(TAG, "Dunno how to parse documentId = (${documentId}), colonsCount > 1 (${colonsCount})")
      return ""
    }

    return spliterator(documentId)
  }

  private fun toDocumentFile(uri: Uri): CachingDocumentFile? {
    try {
      val file = try {
        val docTreeFile = DocumentFile.fromTreeUri(appContext, uri)

        // FIXME: Damn this is hacky as fuck and I don't even know whether it works 100% or not
        if (docTreeFile != null && isBogusTreeUri(uri, docTreeFile.uri)) {
          DocumentFile.fromSingleUri(appContext, uri)
        } else {
          docTreeFile
        }

      } catch (ignored: IllegalArgumentException) {
        DocumentFile.fromSingleUri(appContext, uri)
      }

      if (file == null) {
        Log.e(TAG, "Couldn't convert uri ${uri} into a DocumentFile")
        return null
      }

      if (!file.exists()) {
        return null
      }

      return CachingDocumentFile(appContext, file)
    } catch (e: IllegalArgumentException) {
      Log.e(TAG, "Provided uri is neither a treeUri nor singleUri, uri = $uri")
      return null
    }
  }

  /**
   * When we try to pass a bogus uri to DocumentFile.fromTreeUri it will return us the base tree uri.
   * This means that it cannot be used as a tree uri so we need to use DocumentFile.fromSingleUri
   * instead.
   * */
  @SuppressLint("NewApi")
  private fun isBogusTreeUri(uri: Uri, docTreeUri: Uri): Boolean {
    if (uri.toString() == docTreeUri.toString()) {
      return false
    }

    val docUri = try {
      DocumentsContract.buildDocumentUriUsingTree(
        uri,
        DocumentsContract.getDocumentId(uri)
      )
    } catch (ignored: Throwable) {
      return false
    }

    return docUri.toString() == docTreeUri.toString()
  }

  companion object {
    private const val TAG = "FileManager"
    private const val PRIMARY_STORAGE = "primary"
  }
}