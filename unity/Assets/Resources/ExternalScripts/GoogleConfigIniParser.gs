/**
 * Created by Erxin, Shang(Edwin) on 6/21/2016.
 * JavaScript Configuration file(.ini) content parser, similar to python ConfigParser without I/O operations
 * The license is under GPL-3.0
 * Git repo:https://github.com/shangerxin/config-ini
 * Author homepage: http://www.shangerxin.com
 * Version, 1.2.0
 */


    var error = {
        name   : "ConfigIniParser Error",
        message: "Parse config ini file error"
    };

    var errorNoSection = {
        name   : "ConfigIniParser Error",
        message: "The specify section not found"
    };

    var errorNoOption = {
        name   : "ConfigIniParser Error",
        message: "The specify option not found"
    };

    var errorDuplicateSectionError = {
        name   : "ConfigIniParser Error",
        message: "Found duplicated section in the given ini file"
    };

    var errorNoDelimiter = {
        name   : "ConfigIniParser Error",
        message: "The parameter delimiter is required"
    };

    var DEFAULT_SECTION    = "__DEFAULT_SECTION__";
    var _sectionRegex      = /^\s*\[\s*(\S+)\s*\]\s*$/;
    var _optionRegex       = /\s*(\S+)\s*[=]\s*(.*)\s*/;
// removed :    var _optionRegex       = /\s*(\S+)\s*[=:]\s*(.*)\s*/;
    var _commentRegex      = /^\s*[#;].*/;
    var _emptyRegex        = /^\s*$/;
    var SECTION_NAME_INDEX = 1;
    var OPTION_NAME_INDEX  = 1;
    var OPTION_VALUE_INDEX = 2;
    var NOT_FOUND          = -1;
    var DEFAULT_DELIMITER  = "\n";

    function _findSection(iniStructure, sectionName) {
        var sections = iniStructure.sections;
        for (var i = 0; i < sections.length; i++) {
            var section = sections[i];
            if (section.name == sectionName) {
                return section;
            }
        }
    }

    function _findSectionIndex(iniStructure, sectionName) {
        var sections = iniStructure.sections;
        for (var i = 0; i < sections.length; i++) {
            var section = sections[i];
            if (section.name == sectionName) {
                return i;
            }
        }

        return NOT_FOUND;
    }

    function _findOption(section, optionName) {
        var options = section.options;
        for (var i = 0; i < options.length; i++) {
            var option = options[i];
            if (option.name == optionName) {
                return option;
            }
        }
    }

    function _findOptionIndex(section, optionName) {
        var options = section.options;
        for (var i = 0; i < options.length; i++) {
            var option = options[i];
            if (option.name == optionName) {
                return i;
            }
        }

        return NOT_FOUND;
    }

    function _createSection(name) {
        return {
            name   : name,
            options: []
        };
    }

    function _createOption(name, value) {
        return {
            name : name,
            value: value
        };
    }

    /*
     * Create a ini file parser, the format of ini file could be found at
     * https://en.wikipedia.org/wiki/INI_file
     *
     * Duplicated sections will cause exception, duplicated options will be
     * ignored and only the first one will take effect.
     *
     * @constructor
     * @param {string} delimiter, the line delimiter which is used to separate the lines
     * @return {ConfigIniParser} a ConfigIniParser object
     */
    var ConfigIniParser = function (delimiter) {
        this.delimiter = delimiter? delimiter:DEFAULT_DELIMITER;

        /*
         _init object structure
         {
             sections:[
                 {
                     name:string,
                     options:[
                         {
                             name:string,
                             value:value
                         },
                     ]
                 },
             ]
         }
         */
        this._ini = {
            sections: []
        };

        this._ini.sections.push(_createSection(DEFAULT_SECTION));
      
        this.invalidContent = "";
    };

    /*
     * Create a new section, if the section is already contain in the structure then
     * a duplicated section exception will be thrown
     * @param {string} sectionName a section name defined in ini [section name]
     * @return {object} the created section object
     */
    ConfigIniParser.prototype.addSection = function (sectionName) {
        if (_findSection(this._ini, sectionName)) {
            throw errorDuplicateSectionError;
        }
        else {
            var section = _createSection(sectionName);
            this._ini.sections.push(section);
            return this;
        }
    };

    /*
     * Rename a section, if the section is not in the structure then
     * a duplicated section exception will be thrown
     * @param {string} sectionName a section name defined in ini [section name]
     * @param {string} newSectionName a new name for this section
     * @return {object} the created section object
     */
    ConfigIniParser.prototype.renameSection = function (sectionName, newSectionName) {
        var sectionIndex = _findSectionIndex(this._ini, sectionName);
        if (sectionIndex == NOT_FOUND) {
            throw errorNoSection;
        }
        else {
            this._ini.sections[sectionIndex].name = newSectionName;
            return this._ini.sections[sectionIndex];
        }
    };

    /*
     * Get a specify option value
     * @param {string} sectionName the name defined in ini [section name]
     * @param {string} optionName the name defined in ini option-name = option-value
     * @param {object} defaultValue use the default value if the option does not exist
     * @return {string/object} the string value of the option or the defaultValue
     */
    ConfigIniParser.prototype.get = function (sectionName, optionName, defaultValue) {
        var section = _findSection(this._ini, sectionName ? sectionName : DEFAULT_SECTION);
        if (section) {
            var option = _findOption(section, optionName);
            if (option) {
                return option.value;
            }
        }

        if(defaultValue){
            return defaultValue;
        }
        else{
            throw errorNoOption;
        }
    };

    /*
     * Convert the option value to boolean, if the value is a number then return true if it is
     * not equal to 0; if the value is string and which value is "true"/"false" then will be converted
     * to bool, return true if it is "true";
     * @return {boolean} indicate the value is true or false
     */
    ConfigIniParser.prototype.getBoolean = function (sectionName, optionName) {
        var value = this.get(sectionName ? sectionName : DEFAULT_SECTION, optionName);

        if (isNaN(value)) {
            return String(value).toLowerCase() == "true";
        }
        else {
            return value != 0;
        }
    };

    /*
     * Convert a option value to number
     * @return {number/NaN} number or NaN
     */
    ConfigIniParser.prototype.getNumber = function (sectionName, optionName) {
        return +this.get(sectionName ? sectionName : DEFAULT_SECTION, optionName);
    };

    /*
     * Check a specify section is exist or not
     * @return {boolean} to indicate the result
     */
    ConfigIniParser.prototype.isHaveSection = function (sectionName) {
        return !!_findSection(this._ini, sectionName);
    };

    /*
     * Check an option is exist in a section or not.
     * @return {boolean} boolean
     */
    ConfigIniParser.prototype.isHaveOption = function (sectionName, optionName) {
        var section = _findSection(this._ini, sectionName ? sectionName : DEFAULT_SECTION);
        if (section) {
            var option = _findOption(section, optionName);
            if (option) {
                return true;
            }
        }
        return false;
    };

    /*
     * Get key/value pair of the options in the specify section
     * @param {string} sectionName a section name defined in ini [section name]
     * @return {Array.} an array contain several sub arrays which are composed by optionName,
     * optionValue. The returned array looks like [[optionName0, optionValue0], ...]
     */
    ConfigIniParser.prototype.items = function (sectionName) {
        var section = _findSection(this._ini, sectionName ? sectionName : DEFAULT_SECTION);
        var items   = [];

        for (var i = 0; i < section.options.length; i++) {
            var option = section.options[i];
            items.push([option.name, option.value]);
        }

        return items;
    };

    /*
     * Get all the option names from the specify section
     * @param {string} sectionName a section name defined in ini [section name]
     * @return {Array.} an string array contain all the option names
     */
    ConfigIniParser.prototype.options = function (sectionName) {
        var section = _findSection(this._ini, sectionName ? sectionName : DEFAULT_SECTION);
        if (section) {
            var optionNames = [];
            var options     = section.options;
            var option;
            for (var i = 0; i < options.length; i++) {
                option = options[i];
                optionNames.push(option.name);
            }
            return optionNames;
        }
        else {
            throw errorNoSection;
        }
    };

    /*
     * Remove the specify option from the section if the option exist then remove it
     * and return true else return false
     *
     * @param {string} sectionName a section name defined in ini [section name]
     * @param {string} optionName
     * @return, boolean
     */
    ConfigIniParser.prototype.removeOption = function (sectionName, optionName) {
        var section = _findSection(this._ini, sectionName ? sectionName : DEFAULT_SECTION);
        if (section) {
            var optionIndex = _findOptionIndex(section, optionName);
            if (optionIndex != NOT_FOUND) {
                section.options.splice(optionIndex, 1);
                return true;
            }
        }

        return false;
    };

    /*
     * Remove the specify section if the section exist then remove it
     * and return true else return false
     *
     * @param {string} sectionName
     * @return {boolean}
     */
    ConfigIniParser.prototype.removeSection = function (sectionName) {
        var sectionIndex = _findSectionIndex(this._ini, sectionName);
        if (sectionIndex != NOT_FOUND) {
            this._ini.sections.splice(sectionIndex, 1);
            return true;
        }
        else {
            return false;
        }
    };

    /*
     * Get all the section names from the ini content
     * @return {Array.} an string array
     */
    ConfigIniParser.prototype.sections = function () {
        var sectionNames = [];
        var sections     = this._ini.sections;
        var section;
        for (var i = 0; i < sections.length; i++) {
            section = sections[i];
            if (section.name != DEFAULT_SECTION) {
                sectionNames.push(section.name);
            }
        }
        return sectionNames;
    };

    /*
     * Set a option value, if the option is not exist then it will be added to the section.
     * If the section is not exist an errorNoSection will be thrown
     * @param {string} sectionName, string
     * @param {string} optionName, string
     * @param {string} value, a value should be able to converted to string
     * @return {object} parser object itself
     */
    ConfigIniParser.prototype.set = function (sectionName, optionName, value) {
        var section = _findSection(this._ini, sectionName ? sectionName : DEFAULT_SECTION);
        var option;
        if (section) {
            option = _findOption(section, optionName);
            if (option) {
                option.value = value;
                return this;
            }
            else {
                option = _createOption(optionName, value);
                section.options.push(option);
                return this;
            }
        }
        else {
            throw errorNoSection;
        }
    };

    /*
     * Convert the configuration content to strings the line will the separate with the
     * given line delimiter. A empty line will be added between each section
     *
     * @return {string} the content of configuration
     */
    ConfigIniParser.prototype.stringify = function (delimiter) {
        var lines    = [];
        var sections = this._ini.sections;
        var currentSection;
        var options;
        var currentOption;
        for (var i = 0; i < sections.length; i++) {
            currentSection = sections[i];
            if (currentSection.name != DEFAULT_SECTION) {
                lines.push("[" + currentSection.name + "]");
            }

            options = currentSection.options;
            for (var j = 0; j < options.length; j++) {
                currentOption = options[j];
                lines.push(currentOption.name + "=" + currentOption.value);
            }
            if(lines.length > 0){
                lines.push("");
            }
        }
        return lines.join(delimiter? delimiter:this.delimiter);
    };

    /*
     * Parse a given ini content
     * @param {string} iniContent, a string normally separated with \r\n or \n
     * @return {ConfigIniParser} the parser instance itself
     */
    ConfigIniParser.prototype.parse = function (iniContent) {
        var lines          = iniContent.split(this.delimiter);
        var currentSection = _findSection(this._ini, DEFAULT_SECTION);

        for (var i = 0; i < lines.length; i++) {
            var line = lines[i];
            if (line.match(_commentRegex) || line.match(_emptyRegex)) {
                continue;
            }

            var sectionInfo = line.match(_sectionRegex);
            if (sectionInfo) {
                var sectionName = sectionInfo[SECTION_NAME_INDEX];
                if (_findSection(this._ini, sectionName)) {
                    throw errorDuplicateSectionError;
                }
                else {
                    currentSection = _createSection(sectionName);
                    this._ini.sections.push(currentSection);
                }
                continue;
            }

            var optionInfo = line.match(_optionRegex);
            if (optionInfo) {
                var optionName  = optionInfo[OPTION_NAME_INDEX];
                var optionValue = optionInfo[OPTION_VALUE_INDEX];
                var option      = _createOption(optionName, optionValue);
                currentSection.options.push(option);
                continue;
            }

            // if we are here, it means invalid text is in the ini file, raise an alert
            this.invalidContent += "ALERT: Invalid lines in one scenario content! : "+ iniContent;
            Logger.log("ALERT: Invalid lines in one scenario content! : "+ iniContent);
            // we continue anyway (this will generate spam)
        }
        return this;
    };

