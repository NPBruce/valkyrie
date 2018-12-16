
function ScenariosGrabber(gameType) {

  this._all_scenarios_ini_content ={}
  this._gameType = gameType;

}


ScenariosGrabber.prototype.generate_ini = function generate_ini()
{
    
    // download content and generate a single ini
    this._getContent();
  
    // write file
    this._writeFile();
}


ScenariosGrabber.prototype._getContent = function _getContent() {
  
  // Make a GET request of scenarios list 
  var response = UrlFetchApp.fetch("https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/"+ this._gameType +"/manifest.ini");
  Logger.log("Get list of scenarios:\n" + response.getContentText());
  
  var delimiter = "\n"; //or "\n" for *nux

  parser = new ConfigIniParser(delimiter); //If don't assign the parameter delimiter then the default value \n will be used
  parser.parse(response.getContentText());
   
  var all_scenarios_ini_content="";
  var url="";
  
  parser._ini.sections.forEach(
    function(element) 
    {
     if(element.name!="__DEFAULT_SECTION__")
     {
       Logger.log("get URL of " + element.name);
       var url = parser.get( element.name, "external");  
       var ini_url = url + element.name + '.ini' ;
       
       Logger.log("fetch " + ini_url);
       
       var fetch_OK=true;
       for (var i=0; i<3; i++) 
       {
           try {
              response = UrlFetchApp.fetch(ini_url);
           }
           catch (err)
           {
             Logger.log("fetch error "+ i + " : "+ err + " for URL " + ini_url);
             fetch_OK = false;
           }
         
           if(fetch_OK) i=3;
       }
       
       quest_parser = new ConfigIniParser(delimiter);

       Logger.log("parse");
       quest_parser.parse(response.getContentText());
       
       // add URL in the data
       quest_parser.set("Quest", "url", url);
       
       // rename [Quest] into [ScenarioName]
       quest_parser.renameSection("Quest", element.name);
       
       var text_content = quest_parser.stringify('\n');
       
       Logger.log("Add :" + text_content);
       
       all_scenarios_ini_content += text_content  + "\n";
       
       delete quest_parser;
     }
   });
  
  var date = (new Date()).toISOString();
  date = date.slice(0, date.length-5); // get a clean date
  
  var technical_information = "# Generated the " + date + "'UTC' with " + parser._ini.sections.length + " scenarios\n";

  this._all_scenarios_ini_content = technical_information + all_scenarios_ini_content;
  
  Logger.log("Final ini file is :" + this._all_scenarios_ini_content);

}


//***********************************************************
// Miscellaneous
//***********************************************************

ScenariosGrabber.prototype._writeFile = function _writeFile() 
{
   var filename="Valkyrie"+this._gameType+"Scenarios.ini";
   
   // get the JSON file id, if it exists
   var thisFile = DriveApp.getFileById( SpreadsheetApp.getActive().getId() );
   var sheetFolder = thisFile.getParents().next();
   var inifile = sheetFolder.getFilesByName(filename);
   
   if ( inifile.hasNext() )
   { 
      inifile.next().setContent( this._all_scenarios_ini_content );
   } 
   else
   {
     sheetFolder.createFile(filename, this._all_scenarios_ini_content, "application/ini");
   }
}
