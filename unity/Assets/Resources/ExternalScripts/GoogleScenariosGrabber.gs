
function ScenariosGrabber(gameType) {

  this._all_scenarios_ini_content ={}
  this._gameType = gameType;

}

function fetch_with_retry(uri) 
{
   var fetch_OK=true;
   var response="invalid";
   for (var retry=0; retry<3; retry++) 
   {
       try {
          response = UrlFetchApp.fetch(uri);
       }
       catch (err)
       {
         Logger.log("fetch error "+ retry + " : "+ err + " for URL " + uri);
         fetch_OK = false;
       }
     
       if(fetch_OK) retry=3;
   }

   return response;
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
  var response = fetch_with_retry("https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/"+ this._gameType +"/manifest.ini");
  
  if(response=="invalid")
  {
    throw "Invalid get request, stopping here";
  }
  
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
       
       Logger.log("fetch ini :" + ini_url);
       
       response = fetch_with_retry(ini_url);

       if(response=="invalid")
       {
          throw "Invalid get request, stopping here";
       }
  
       quest_parser = new ConfigIniParser(delimiter);

       Logger.log("parse");
       quest_parser.parse(response.getContentText());
       
       // add URL in the data
       quest_parser.set("Quest", "url", url);
       
       // This "https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/MoM/ExoticMaterial/ExoticMaterial.ini"
       // should become this : "https://api.github.com/repos/NPBruce/valkyrie-store/commits?path=MoM/ExoticMaterial/ExoticMaterial.valkyrie"
       var regex = /https:\/\/raw.githubusercontent.com\/(.+?\/.+?)\/.+?\/(.+\/*.*).ini/;
       var package_url = ini_url.replace(regex, 'https://api.github.com/repos/$1/commits?path=$2.valkyrie')
       
       Logger.log("fetch commit package :" + package_url);
       
       response = fetch_with_retry(package_url);

       if(response=="invalid")
       {
          throw "Invalid get request for package, stopping here";
       }
       
       // Make request to API and get response before this point.
       var commit_json = response.getContentText();
       var commit_data = JSON.parse(commit_json);
       Logger.log("Latest commit date is : " + commit_data[0].commit.committer.date);
      
       // add latest update date in the data
       quest_parser.set("Quest", "latest_update", commit_data[0].commit.committer.date);

       // rename [Quest] into [ScenarioName]
       quest_parser.renameSection("Quest", element.name);

       // Create text from .ini object
       var text_content = quest_parser.stringify('\n');
       
       Logger.log("Add :" + text_content);
       
       all_scenarios_ini_content += text_content  + "\n";
       
       delete quest_parser;
     }
   });
  
  var date = (new Date()).toISOString();
  date = date.slice(0, date.length-5); // get a clean date
  
  var technical_information = "# Generated the " + date + "'UTC' with " + (parser._ini.sections.length - 1) + " scenarios\n";

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
