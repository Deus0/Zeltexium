
window.onload = function () {
    window.focus();
}

window.onclick = function () {
    window.focus();
}

// Normal Synching of files
function SyncFiles()
 {
     //alert("Syncing");
     FS.syncfs(false,function (err)
     {
         // handle callback
     });
}

// Export an image, using Base64 String
function ExportImage(FileName, Data)
{
    console.log("Exporting File: " + FileName);
    var textToSaveAsBlob = new Blob([base64ToArrayBuffer(Data)], { type: "image/png" });
    var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
    var fileNameToSaveAs = FileName;
    var downloadLink = document.createElement("a");
    downloadLink.download = fileNameToSaveAs;
    downloadLink.innerHTML = "Download File";
    downloadLink.href = textToSaveAsURL;
    downloadLink.onclick = destroyClickedElement;
    downloadLink.style.display = "none";
    document.body.appendChild(downloadLink);
    downloadLink.click();
}

// Export a file
function Export(FileName, Data)
{
    console.log("Exporting File: " + FileName);
    var textToSaveAsBlob = new Blob([Data], { type: "text/plain" });
    var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
    var fileNameToSaveAs = FileName;
    var downloadLink = document.createElement("a");
    downloadLink.download = fileNameToSaveAs;
    downloadLink.innerHTML = "Download File";
    downloadLink.href = textToSaveAsURL;
    downloadLink.onclick = destroyClickedElement;
    downloadLink.style.display = "none";
    document.body.appendChild(downloadLink);
    downloadLink.click();
}

function destroyClickedElement(event)
{
    document.body.removeChild(event.target);
}

var IsImportFile = false;
var MyObjectName = "";
var MyFunctionName = "";
var ImportFileType = "";
var ImportElement;

// Create a button and activate it
function Import(ObjectName, FunctionName, FileType)
{
    document.getElementById('canvas').addEventListener("mouseup", Import2, false);//
    //document.getElementById('canvas').onmouseover
    console.log("0 - Importing File");
    IsImportFile = true;
    MyFunctionName = FunctionName;
    MyObjectName = ObjectName;
    ImportFileType = FileType;
    ImportElement = document.createElement("input");
    ImportElement.setAttribute('id', 'ImportElement');
    ImportElement.type = "file";
    ImportElement.style.visibility = 'hidden';
    ImportElement.accept = "." + ImportFileType;
    ImportElement.onchange = ReadFile;
    ImportElement.multiple = "multiple";    // multiple
    console.log("1 - Importing File: " + FileType + "-" + ObjectName + ":" + FunctionName);
}
// The second part!! Click the Import Element!
function Import2()
{
    console.log("2.0 Import2!");
    if (IsImportFile == true)
    {
        IsImportFile = false;
        ImportElement.click();
        document.getElementById('canvas').removeEventListener("mouseup", Import2);
        console.log("2 UploadLink? " + (document.getElementById('ImportElement') != null));
    }
}

//var FileIndex = 0;
function ReadFile(MyEvent)
{
    console.log("3 - Reading File: " + MyEvent.target.files[0]);
    //Retrieve the first (and only!) File from the FileList object
    for (var i = 0; i < MyEvent.target.files.length; i++)
    {
        var MyFile = MyEvent.target.files[i];
        if (MyFile)
        {
            var MyFileReader = new FileReader();
            // Pass in the ReadEvent and MyFile
            MyFileReader.onload = (function (MyFile)
            {
                var MyFileName = MyFile.name;
                return function (ReadEvent) {
                    //console.log(fileName);
                    //console.log(e.target.result);
                    var MyData = ReadEvent.target.result; 
                    console.log("4 - Uploading the file:      "
                          + "Name [" + MyFileName + "]      "
                          + "Size [" + MyData.size + "] to "
                          + MyObjectName + ":" + MyFunctionName
                    );
                    if (ImportFileType == "png" || ImportFileType == "zip")
                    {
                        var MyPacket = MyFileName + "\n" + arrayBufferToBase64(MyData);
                        //SendMessage(MyObjectName, MyFunctionName + "FileName", MyFile.name);
                        SendMessage(
                            MyObjectName,
                            MyFunctionName,
                            MyPacket);
                    }
                    else
                    {
                        var MyPacket = MyFileName + "\n" + MyData;
                        //SendMessage(MyObjectName, MyFunctionName + "FileName", MyFile.name);
                        SendMessage(MyObjectName, MyFunctionName, MyPacket);
                    }
                };
            })(MyFile);
            if (ImportFileType == "png" || ImportFileType == "zip")
            {
                MyFileReader.readAsArrayBuffer(MyFile);
            }
            else
            {
                MyFileReader.readAsText(MyFile);
            }
        }
        else {
            alert("Failed to load file");
        }
    }
}
// Converts the array bufer to Base64 string to send to unity!
function arrayBufferToBase64(ab)
{
    var dView = new Uint8Array(ab);   //Get a byte view        

    var arr = Array.prototype.slice.call(dView); //Create a normal array        

    var arr1 = arr.map(function (item) {
        return String.fromCharCode(item);    //Convert
    });

    return window.btoa(arr1.join(''));   //Form a string
}
function base64ToArrayBuffer(base64) {
    var binary_string = window.atob(base64);
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
}


/*function loadFileAsText()
{
    var fileToLoad = document.getElementById("fileToLoad").files[0];

    var fileReader = new FileReader();
    fileReader.onload = function (fileLoadedEvent) {
        var textFromFileLoaded = fileLoadedEvent.target.result;
        document.getElementById("inputTextToSave").value = textFromFileLoaded;
    };
    fileReader.readAsText(fileToLoad, "UTF-8");
}*/