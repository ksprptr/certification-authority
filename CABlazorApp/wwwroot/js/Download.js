function Download(filePath, fileName, fileType) {
    fetch(filePath)
        .then(response => response.blob())
        .then(blob => {
            const file = new File([blob], fileName, { type: fileType });
            const element = document.createElement('a');
            element.href = URL.createObjectURL(file);
            element.download = fileName;
            element.style.display = 'none';
            document.body.appendChild(element);
            element.click();
            document.body.removeChild(element);
        })
        .catch(error => console.error('Error downloading file:', error));
}