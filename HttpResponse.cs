using System;
using System.Text;
using System.Collections;

namespace DirectoryBrowser {

	public class HttpResponse {

		private static String VERSION = "1.1";
		private int code;
		private String status;
		private Hashtable headers;
		private String body;

		public HttpResponse(int code, string status,String contentType){
			this.code = code;
			this.status = status;
			this.headers = new Hashtable();
			this.headers.Add("Content-Type",contentType);
		}

		public HttpResponse(int code, string status) : this(code,status,"text/html"){}

		public bool HasHeader(String name){
			return this.headers.ContainsKey(name);
		}

		public void SetHeader(String key,String value){
			if(this.HasHeader(key)){
				this.headers[key] = value;
			}else{
				this.headers.Add(key,value);
			}
		}

		public void SetBody(String body){
			this.body = body;
		}

		public void AppendBody(String text){
			this.body += text;
		}

		private String GetHeaderString(){
			String str = "";
			foreach(DictionaryEntry e in this.headers){
				str += String.Format("{0}: {1}\n",e.Key,e.Value);
			}
			return str;
		}

		public override String ToString(){
			return String.Format(
				"HTTP/{0} {1} {2}\n{3}\n{4}",
				VERSION,
				code,
				status,
				GetHeaderString(),
				body
			);
		}

	}

}