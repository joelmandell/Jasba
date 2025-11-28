// Quill.js WYSIWYG Editor Integration for Security Plans

let quillInstance = null;

window.initQuillEditor = function (initialContent) {
    // Ensure Quill is loaded
    if (typeof Quill === 'undefined') {
        console.error('Quill is not loaded. Please include Quill.js CDN in your _Layout.cshtml or index.html');
        return;
    }

    // Destroy existing instance if any
    if (quillInstance) {
        try {
            const container = document.querySelector('#editor-container');
            if (container) {
                container.innerHTML = '';
            }
        } catch (e) {
            console.error('Error destroying previous Quill instance:', e);
        }
    }

    // Initialize Quill with full toolbar
    const toolbarOptions = [
        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
        [{ 'font': [] }],
        [{ 'size': ['small', false, 'large', 'huge'] }],
        
        ['bold', 'italic', 'underline', 'strike'],
        [{ 'color': [] }, { 'background': [] }],
        [{ 'script': 'sub'}, { 'script': 'super' }],
        
        ['blockquote', 'code-block'],
        [{ 'list': 'ordered'}, { 'list': 'bullet' }],
        [{ 'indent': '-1'}, { 'indent': '+1' }],
        [{ 'direction': 'rtl' }],
        
        [{ 'align': [] }],
        ['link', 'image', 'video'],
        
        ['clean']
    ];

    try {
        quillInstance = new Quill('#editor-container', {
            theme: 'snow',
            modules: {
                toolbar: toolbarOptions
            },
            placeholder: 'Write your security plan guidelines here...'
        });

        // Set initial content if provided
        if (initialContent && initialContent.trim() !== '') {
            quillInstance.root.innerHTML = initialContent;
        }

        console.log('Quill editor initialized successfully');
    } catch (error) {
        console.error('Error initializing Quill:', error);
    }
};

window.getQuillContent = function () {
    if (quillInstance) {
        return quillInstance.root.innerHTML;
    }
    return '';
};

window.destroyQuillEditor = function () {
    if (quillInstance) {
        try {
            const container = document.querySelector('#editor-container');
            if (container) {
                container.innerHTML = '';
            }
            quillInstance = null;
            console.log('Quill editor destroyed');
        } catch (error) {
            console.error('Error destroying Quill editor:', error);
        }
    }
};

// Cleanup on page navigation
window.addEventListener('beforeunload', function () {
    if (quillInstance) {
        quillInstance = null;
    }
});
