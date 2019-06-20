using System;
using System.Text;
using System.Collections;

namespace DirectoryBrowser {

	public class HtmlDocument {

		private HtmlHeadElement head;
		private HtmlBodyElement body;

		public HtmlDocument(){
			this.head = new HtmlHeadElement();
			this.body = new HtmlBodyElement();
		}

		public HtmlHeadElement GetHead(){
			return this.head;
		}
		public HtmlBodyElement GetBody(){
			return this.body;
		}

		public override String ToString(){
			return String.Format(
				"<!DOCTYPE html><html>{0}{1}</html>",
				this.head.ToString(),
				this.body.ToString()
			);
		}

	}

}