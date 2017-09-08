var FMWebGL = {

	data: 0,	// Temporary data array
    
	// Reads the file and returns the length, the content is stored in "data" array.
	ReadFileLen: function(url)
	{
		// This function stops execution while downloading data:
		this.data = new Uint8Array (0);	// Default empty array
		var req = new XMLHttpRequest();
		req.open("GET", Pointer_stringify(url), false);	// Synchronous file download
		req.overrideMimeType("text/plain; charset=x-user-defined");	//Avoid interpretation
		req.send();
		// Error status (404) can't be captured, the browser crashes instead of throwing exception:
		if (req.status === 200)
		{
			this.data = new Uint8Array(req.response.length);
			// Load data into "data" array:
			for(var i=0; i<req.response.length; i++)
				this.data[i] = req.response.charCodeAt(i) & 0xFF;	// Force 8bit data
		}
		return this.data.length;
	},
	
	// Retrieve "data" array content:
	ReadData: function()
    {
		var ptr = _malloc(this.data.length+1);
		writeArrayToMemory(this.data, ptr);
		return ptr;
	},
	
	// Saves the files into DB:
	SyncFiles : function()
	{
		FS.syncfs(false, function (err) {} );
	},
	
	// Just a simple tool for debugging:
	ShowMessage: function(msg)
	{
		var _msg = Pointer_stringify(msg);
		window.alert(_msg);
	},
	
	////////////////////// COOKIES ///////////////////////////////////////////////////
	
	// Reads the cookie and returns the length, the content is stored in "data" array:
	ReadCookieLen: function(name)
    {
		this._name = Pointer_stringify(name) + "=";
		var dc = document.cookie;
		this.data = new Uint8Array (0);	// Default empty array
		if(dc.length > 0)
		{
			var begin = dc.indexOf(this._name);	// Search for a cookie
			if(begin != -1)
			{
				// Ok, there is a cookie:
				var returnStr = "";
				begin += this._name.length;
				end = dc.indexOf(";", begin);
				if(end == -1)
					end = dc.length;
				returnStr = unescape(dc.substring(begin, end));	// Delete escape chars
				this.data = new Uint8Array(returnStr.length);
				// Load data into "data" array:
				for(var i=0; i < returnStr.length; i++)
					this.data[i] = returnStr.charCodeAt(i) & 0xFF;	// Force 8bit data
			}
		}
		return this.data.length;
	},
	
	// Writes data into a cookie:
    WriteCookie: function(name, value, expires, sec, path, domain)
    {
		var date = new Date();
		this._name = Pointer_stringify(name);
		var _value = Pointer_stringify(value);
		var _expires = Pointer_stringify(expires);
		var _path = Pointer_stringify(path);
		var _domain = Pointer_stringify(domain);
		
		date.setTime(date.getTime()+(20*365*24*60*60*1000));	// Sets 20 years duration
		document.cookie = this._name + "=" + escape(_value) +
						((_expires == "") ? "; expires=" + date.toGMTString() : "; expires=" + _expires.toGMTString()) +
						((_path == "") ? "" : "; path=" + _path) +
						((_domain == "") ? "" : "; domain=" + _domain) +
						((sec == 0) ? "" : "; secure");
    },
	
	// Deletes the selected cookie:
    DeleteCookie: function(name, path, domain)
    {
		this._name = Pointer_stringify(name);
		var _path = Pointer_stringify(path);
		var _domain = Pointer_stringify(domain);
		var cname = this._name + "=";
		var dc = document.cookie;
		
		if(dc.length > 0)
		{
			var begin = dc.indexOf(cname);
			if(begin != -1)
			{
				document.cookie = this._name + "=" +
								((_path == "") ? "" : "; path=" + _path) +
								((_domain == "") ? "" : "; domain=" + _domain) +
								"; expires=Thu, 01-Jan-70 00:00:01 GMT";
			}
		}
    },
	
	// Deletes all cookies for this application:
	DeleteAllCookies: function()
	{
		var cookies = document.cookie.split(";");
		
		for (var i = 0; i < cookies.length; i++)
		{
			var cookie = cookies[i];
			var eqPos = cookie.indexOf("=");
			var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
			document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
		}
	}
};

mergeInto(LibraryManager.library, FMWebGL);
