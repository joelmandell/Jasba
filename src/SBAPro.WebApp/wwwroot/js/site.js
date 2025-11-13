// SBA Pro Site JavaScript Utilities

window.downloadPdf = (base64, filename) => {
    const link = document.createElement('a');
    link.href = 'data:application/pdf;base64,' + base64;
    link.download = filename;
    link.click();
};
