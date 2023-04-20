function Download(fileName, fileType, fileContent) {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:' + fileType + ';base64,' + fileContent);
    element.setAttribute('download', fileName);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
}