window.adcArchivos = window.adcArchivos || {};

window.adcArchivos.abrirDesdeBase64 = (nombreArchivo, contentType, base64Data) => {
    const bytes = atob(base64Data);
    const numeros = new Array(bytes.length);
    for (let i = 0; i < bytes.length; i++) {
        numeros[i] = bytes.charCodeAt(i);
    }

    const arregloBytes = new Uint8Array(numeros);
    const blob = new Blob([arregloBytes], { type: contentType || "application/octet-stream" });
    const url = URL.createObjectURL(blob);

    const ventana = window.open(url, "_blank");
    if (!ventana) {
        const enlace = document.createElement("a");
        enlace.href = url;
        enlace.download = nombreArchivo || "archivo";
        document.body.appendChild(enlace);
        enlace.click();
        document.body.removeChild(enlace);
    }

    setTimeout(() => URL.revokeObjectURL(url), 10000);
};

window.adcArchivos.descargarDesdeBase64 = (nombreArchivo, contentType, base64Data) => {
    const bytes = atob(base64Data);
    const numeros = new Array(bytes.length);
    for (let i = 0; i < bytes.length; i++) {
        numeros[i] = bytes.charCodeAt(i);
    }

    const arregloBytes = new Uint8Array(numeros);
    const blob = new Blob([arregloBytes], { type: contentType || "application/octet-stream" });
    const url = URL.createObjectURL(blob);

    const enlace = document.createElement("a");
    enlace.href = url;
    enlace.download = nombreArchivo || "archivo";
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);

    setTimeout(() => URL.revokeObjectURL(url), 10000);
};
