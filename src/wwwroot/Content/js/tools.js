(function (window) {
    var Method = {};
    //绑定数组到select控件上
    Method.bindArrayToSelect = function (arrs, selectId) {
        if (Array.isArray(arrs)) {
            var ddlelem = document.getElementById(selectId);
            ddlelem.options.length = 0;
            for (var i = 0; i < arrs.length; i++) {
                ddlelem.options[i] = new Option(arrs[i].name, arrs[i].id);
            }
        }
    }
    //获取复选控件ddlList的值/获取复选框名称
    Method.getSelectArray = function (selectId, type) {
        var ddlKeyCode = document.getElementById(selectId);
        var value = '', name = '';

        if (ddlKeyCode.multiple) {
            for (var i = 0; i < ddlKeyCode.length; i++) {
                if (ddlKeyCode.options[i].selected) {
                    value += ddlKeyCode.options[i].value + ",";
                    name += ddlKeyCode.options[i].text + ",";
                }
            }
        }
        else {
            value = ddlKeyCode.value;
            name = ddlKeyCode.options[ddlKeyCode.selectedIndex].text;
        }
        if (type && type === "name") {
            return name;
        }
        else {
            return value;
        }
    }
    //要获取的参数名称
    Method.getQueryString = function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) {
            return r[2];
        }
        return null
    };
    
    //全球唯一标识符
    Method.uuid = function (len, radix) {
        var chars = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz'.split('');
        var uuid = [], i;
        radix = radix || chars.length;
        if (len) {
            // Compact form
            for (i = 0; i < len; i++) uuid[i] = chars[0 | Math.random() * radix];
        } else {
            // rfc4122, version 4 form
            var r;

            // rfc4122 requires these characters
            uuid[8] = uuid[13] = uuid[18] = uuid[23] = '-';
            uuid[14] = '4';

            // Fill in random data.  At i==19 set the high bits of clock sequence as
            // per rfc4122, sec. 4.1.5
            for (i = 0; i < 36; i++) {
                if (!uuid[i]) {
                    r = 0 | Math.random() * 16;
                    uuid[i] = chars[(i == 19) ? (r & 0x3) | 0x8 : r];
                }
            }
        }
        return uuid.join('');
    }
    //自定义唯一id编号，yymmddhhmmssfffffff+uuid10位16进制
    Method.newId = function () {
        var date = new Date();
        var pad2 = function (n) { return n < 10 ? '0' + n : n }
        return date.getFullYear().toString() +
            pad2(date.getMonth() + 1) +
            pad2(date.getDate()) +
            pad2(date.getHours()) +
            pad2(date.getMinutes()) +
            pad2(date.getSeconds()) +
            pad2(date.getMilliseconds()) + Method.uuid(10, 16);
    }
    Method.Trim = function (str) {
        return str.replace(/(^\s*)|(\s*$)/g, "");

    }
   
  
    //本地读取图片文件并显示出来
    Method.lookLogo = function (fileUiName, imgUiName) {
        //获取file文件控件
        var txtfile = document.getElementById(fileUiName);
        //获取选择文件的数组
        var fileList = txtfile.files;
        for (var i = 0; i < fileList.length; i++) {
            var file = fileList[i];
            //图片类型验证第二种方式
            if (/image\/\w+/.test(file.type)) {
                //新建一个文件阅读对象，读取转换为base64格式
                var reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = function (e) {
                    var result = reader.result;
                    document.getElementById(imgUiName).src = result;
                    // console.log(result);
                };
                reader.onprogress = function (evt) {
                    console.log(Math.round(evt.loaded / evt.total * 100) + "%");//动态读取进度
                };

            }
            else {
                alert(file.name + '不是图片');
            }
        }
    }
    Method.getFileExt = function (fileUiName) {
        var file = document.getElementById(fileUiName).files[0];//单文件上传
        var ext = "";
        if (file) {
            ext = file.name.substring(file.name.lastIndexOf('.'));
        }
        return ext;
    }
    //关键代码上传到服务器
    Method.uploadFile = function (fileUiName, serverUrl,callback) {
        var file = document.getElementById(fileUiName).files[0];
        var name = "";
        if (file) {
            var reader = new FileReader();
            reader.readAsArrayBuffer(file);
            reader.onload = function () {
                var blob = new Blob([reader.result], { type: 'image/*' });
                var fd = new FormData();
                fd.append('file', blob);
                fd.append('filename', file.name);
                fd.append('maxsize', 1024 * 1024 * 4);
                fd.append('isClip', -1);
                var xhr = new XMLHttpRequest();
                xhr.open('post', serverUrl, true);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState == 4 && xhr.status == 200) {
                        //转换为前端可访问的对象因为后端返回一个jsonresult对象前端不能直接解析获取data,status之类
                        var data = eval('(' + xhr.responseText + ')');
                        name = data;
                        if (callback) {
                            setTimeout(function () {
                                callback(data);
                            }, 100)
                        }
                    }
                }
                xhr.send(fd);
            }
        }
        return name;
    }
    Method.editor = function (contentId, uploadUrl) {
        var E = window.wangEditor;
        var editor = new E('#editorMenu', '#editor');
        editor.customConfig.onchange = function (html) {
            // 监控变化，同步更新到 textarea
            document.getElementById(contentId).value = html;
            //document.getElementById("Speak").value = editor.txt.text();
        }
        //配置图片上传
        editor.customConfig.uploadImgServer = uploadUrl;  // 上传图片到服务器
        // 3M
        editor.customConfig.uploadImgMaxSize = 3 * 1024 * 1024;
        // 限制一次最多上传 5 张图片
        editor.customConfig.uploadImgMaxLength = 10;
        // 自定义文件名
        // 将 timeout 时间改为 3s
        editor.customConfig.uploadImgTimeout = 5000;
        editor.customConfig.uploadImgHooks = {
            before: function (xhr, editor, files) {
                // 图片上传之前触发
                // xhr 是 XMLHttpRequst 对象，editor 是编辑器对象，files 是选择的图片文件

                // 如果返回的结果是 {prevent: true, msg: 'xxxx'} 则表示用户放弃上传
                // return {
                //     prevent: true,
                //     msg: '放弃上传'
                // }
                // alert("前奏");
            },
            success: function (xhr, editor, result) {
                // 图片上传并返回结果，图片插入成功之后触发
                // xhr 是 XMLHttpRequst 对象，editor 是编辑器对象，result 是服务器端返回的结果
                // var url = result.data.url;
                // alert(JSON.stringify(url));
                // editor.txt.append(url);
                // alert("成功");
            },
            fail: function (xhr, editor, result) {
                // 图片上传并返回结果，但图片插入错误时触发
                // xhr 是 XMLHttpRequst 对象，editor 是编辑器对象，result 是服务器端返回的结果
                alert("失败");
            },
            error: function (xhr, editor) {
                // 图片上传出错时触发
                // xhr 是 XMLHttpRequst 对象，editor 是编辑器对象
                // alert("错误");
            },
            // 如果服务器端返回的不是 {errno:0, data: [...]} 这种格式，可使用该配置
            // （但是，服务器端返回的必须是一个 JSON 格式字符串！！！否则会报错）
            customInsert: function (insertImg, result, editor) {
                // 图片上传并返回结果，自定义插入图片的事件（而不是编辑器自动插入图片！！！）
                // insertImg 是插入图片的函数，editor 是编辑器对象，result 是服务器端返回的结果
                // 举例：假如上传图片成功后，服务器端返回的是 {url:'....'} 这种格式，即可这样插入图片：
                if (result && result.length > 0)
                    for (var i = 0; i < result.length; i++) {
                        var url = result[i];
                        insertImg(url);
                        console.log(url);
                    }
                // result 必须是一个 JSON 格式字符串！！！否则报错
            }
        }
        editor.create();
        return editor;
    }

    window.Method = Method;
   
})(window)

