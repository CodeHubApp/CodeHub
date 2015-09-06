function diff(base_text, new_text) {
    var base = difflib.stringAsLines(base_text);
    var newtxt = difflib.stringAsLines(new_text);
    var sm = new difflib.SequenceMatcher(base, newtxt);
    var opcodes = sm.get_opcodes();
    var diffoutputdiv = document.body;
    while (diffoutputdiv.firstChild) diffoutputdiv.removeChild(diffoutputdiv.firstChild);
    diffoutputdiv.appendChild(diffview.buildView({
        baseTextLines: base,
        newTextLines: newtxt,
        opcodes: opcodes,
        baseTextName: "Base Text",
        newTextName: "New Text",
        contextSize: 3,
        viewType: 1
    }));
    
    $('td').each(function(i, el) {
        $(el).click(function() {
            invokeNative("comment", {"line_to": $(el).parent().data('to'), "line_from": $(el).parent().data('from')});
        });
    });
}

function loadFileAsPatch(path) {
	$.get(path, function(data) {
		patch(data);
	});
}

function escapeHtml(data) {
	return $('<div/>').text(data).html(); 
}

function patch(p) {
	var $body = $('body');
	var $table = $("<table class='diff inlinediff'></table>");

	function createRow(x, y, type, line, lineNum) {
		$table.append("<tr data-to='" + lineNum + "' data-x='" + x + "' data-y='" + y + "'><th>" + x + "</th><th>" + y + "</th><td class='" + type + "'>" + escapeHtml(line) + "</td></tr>");
	};
	
	var lines = p.split("\n");
	var baseLine = 0;
	var newLine = 0;
	for (var i = 0; i < lines.length; i++) {
		var line = lines[i];
		if (line.lastIndexOf("@@", 0) === 0) {
			createRow("...", "...", "skip", line, i);
			var r = /@@ -(\d+).+\+(\d+)/i;
			var arr = r.exec(line);
			baseLine = arr[1];
			newLine = arr[2];
		} else {
			if (line.lastIndexOf("+", 0) === 0) {
				createRow("", newLine, "insert", line, i);
				newLine++;
			} else if (line.lastIndexOf("-", 0) === 0) {
				createRow(baseLine, "", "delete", line, i);
				baseLine++;
			} else {
				createRow(baseLine, newLine, "equal", line, i);
				baseLine++;
				newLine++;
			}
		}
	}
	
	$body.append($table);
	
    $('td:not(.skip)').each(function(i, el) {
        $(el).click(function() {
        	var fileLine = $(el).parent().data('y');
        	if (fileLine === "")
        		fileLine = $(el).parent().data('x')
            invokeNative("comment", {"patch_line": $(el).parent().data('to'), "file_line": fileLine});
        });
    });
}

function invokeNative(functionName, args) {
    try
    {
	    var iframe = document.createElement('IFRAME');
	    iframe.setAttribute('src', 'app://' + functionName + '#' + JSON.stringify(args));
	    document.body.appendChild(iframe);
	    iframe.parentNode.removeChild(iframe);
	    iframe = null;  
    }
    catch (err)
    {
    	alert(err.message);
    }
}

function setComments(comments) {

    $('tr.comment').remove();

    for (var i = 0; i < comments.length; i++) {
        var comment = comments[i];
        var $comment = $("<tr data-id='" + comment.id + "' class='comment'><td colspan='3'><div class='inner'><header><img src='" + comment.avatar + "' />" + comment.user + "</header><div class='content'>" + comment.content + "</div></div></td></tr>");
        
        if (comment['line_to'] != null) {
            $("tr[data-to='" + comment.line_to + "']").after($comment);
        }
        else if (comment['line_from'] != null) {
            $("tr[data-from='" + comment.line_from + "']").after($comment);
        }
        else if (comment['parent'] != null) {
            $("tr[data-id='" + comment.parent + "']").after($comment);
        }
    }
}

window.onload = function() { document.location.href = 'app://ready'};
