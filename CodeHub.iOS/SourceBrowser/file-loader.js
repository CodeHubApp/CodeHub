(function(){

if (!self.document || !document.querySelector) {
        return;
}

var Extensions = {
        'js': 'javascript',
        'html': 'xml',
        'rb': 'ruby',
        'xml': 'markup',
        'py': 'python',
        'm': 'objectivec',
        'pl': 'perl',
        'md': 'markdown',
        'as': 'actionscript'
};
        
Array.prototype.slice.call(document.querySelectorAll('pre[data-src]')).forEach(function(pre) {
        var src = pre.getAttribute('data-src');
        var extension = (src.match(/\.(\w+)$/) || [,''])[1];
        var language = Extensions[extension] || extension;
        
        var code = document.createElement('code');

        code.className = 'lineNumbers language-' + language;
        
        pre.textContent = '';
        
        code.textContent = 'Loading…';
        
        pre.appendChild(code);
        
        var xhr = new XMLHttpRequest();
        
        xhr.open('GET', src, true);

        xhr.onreadystatechange = function() {
                if (xhr.readyState == 4) {
                        
                        if (xhr.status < 400 && xhr.responseText) {
                                code.textContent = xhr.responseText;
								hljs.highlightBlock(code)
                        }
                        else if (xhr.status >= 400) {
                                code.textContent = '✖ Error ' + xhr.status + ' while fetching file: ' + xhr.statusText;
                        }
                        else {
                                code.textContent = '✖ Error: File does not exist or is empty';
                        }
                }
        };
        
        xhr.send(null);
});

})();