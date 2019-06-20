using System;
using System.Text;
using System.Collections;

namespace DirectoryBrowser {

	public class HtmlBaseElement {

		private String tagName;
		private String textContent;
		private Hashtable attributes;
		private ArrayList classNames;
		private ArrayList children;

		public HtmlBaseElement(String tagName,String textContent){
			this.tagName = tagName;
			this.textContent = textContent;
			this.attributes = new Hashtable();
			this.classNames = new ArrayList();
			this.children = new ArrayList();
		}

		public void SetInnerText(String textContent){
			this.textContent = textContent;
		}

		public void SetTextContent(String textContent){
			this.SetInnerText(textContent);
		}

		public bool HasAttribute(String key){
			return this.attributes.ContainsKey(key);
		}

		public void SetAttribute(String key,String value){
			if(key == "class"){
				if(this.classNames.Contains(value)) return;
				this.classNames.Add(value); 
			}else if(this.HasAttribute(key)){
				this.attributes[key] = value;
			}else{
				this.attributes.Add(key,value);
			}
		}

		public void AppendChild(HtmlBaseElement child){
			this.children.Add(child);
		}

		public bool HasChild(){
			return this.children.Count > 0;
		}

		public bool HasClasses(){
			return this.classNames.Count > 0;
		}

		private String[] GetClassNames(){
			String[] names = new String[this.classNames.Count];
			int i=0; foreach(String name in this.classNames) names[i++] = name;
			return names;
		}

		private String GetAttributesString(){
			String str = "";
			foreach(DictionaryEntry attr in this.attributes){
				str += String.Format(" {0}=\"{1}\"",attr.Key,attr.Value);
			}
			if(!this.HasClasses()) return str;
			String classes = String.Join(",",this.GetClassNames());
			return String.Format("{0} class=\"{1}\"",str,classes);
		}

		private String OutputChildren(){
			String str = "";
			foreach(HtmlBaseElement child in children){
				str += child.ToString();
			}
			return str;
		}

		private String GetContent(){
			if(!this.HasChild()) return this.textContent;
			return this.OutputChildren();
		}

		public override String ToString(){
			return String.Format(
				"<{0}{1}>{2}</{0}>",
				this.tagName,
				GetAttributesString(),
				GetContent()
			);
		}

	}

}