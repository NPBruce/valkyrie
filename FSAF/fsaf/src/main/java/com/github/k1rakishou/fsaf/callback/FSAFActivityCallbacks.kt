package com.github.k1rakishou.fsaf.callback

import android.content.Intent

interface FSAFActivityCallbacks {
  fun fsafStartActivityForResult(intent: Intent, requestCode: Int)
}