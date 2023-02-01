function DownloadFile(fileName, fileType, fileContent) {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:' + fileType + ';base64,' + fileContent);
    element.setAttribute('download', fileName);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
}

function DownloadPFXFile(fileName, fileContent) {
    if (fileName.endsWith(".pfx")) {
        var byteArray = new Uint8Array(fileContent);
        var fileContentBase64 = window.btoa(String.fromCharCode.apply(null, byteArray));
        var element = document.createElement('a');
        element.setAttribute('href', 'data:application/x-pkcs12;base64,' + fileContentBase64);
        element.setAttribute('download', fileName);
        element.style.display = 'none';
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    } else {
        console.error("Error: Only PFX files are allowed.");
    }
}

function preventDefault(event) {
    event.preventDefault();
}