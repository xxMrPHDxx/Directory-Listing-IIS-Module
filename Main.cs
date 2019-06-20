using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace DirectoryBrowser {

	public class Program {

		private static String ConcatString(params String[] str){
			return String.Join("",str);
		}

		private static bool IsImage(String extension){
			switch(extension){
				case "png":
				case "jpeg":
				case "webp":
				case "gif":
				case "jpg":
					return true;
			}
			return false;
		}

		private static bool IsSourceCode(String extension){
			switch(extension){
				case "cs": // C Sharp
				case "cpp": // C++
				case "c": // C
				case "java": // Java
				case "r": // R Script
				case "py": // Python
				case "rb": // Ruby
					return true;
			}
			return false;
		}

		private static String GetExtension(String extension){
			if(IsImage(extension)){
				return String.Format("image/{0}",extension);
			}else if(IsSourceCode(extension)){
				return "text/plain";
			}
			else throw new Exception("Ops! Not defined.");
		}

		private static String DIRECTORY_FORMAT = "<div><div class='icon folder'></div><a href='{1}' class='fname'>{0}</a><div></div></div>";
		private static String FILE_FORMAT = "<div><div class='icon file'></div><a href='{1}' class='fname'>{0}</a><div class='icon download' onclick='download_file(\"{1}\",\"{0}\")'></div></div>";

		private static HttpResponse response;

		public static String HTTP_HEADER = "HTTP/1.1 200 OK";
		public static String HTML = String.Format(
			"<html><head><style type='text/css'>\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n</style><script type='text/javascript'>{7}</script></head><body><div class='content'>",
			"div.content { display: grid; grid-auto-flow: row; }",
			"div.content>div { display: grid; grid-gap: 20px; padding: 10px; grid-template-columns: 32px auto 32px; }",
			"div.content>div>* { display: flex; align-items: center; }",
			".icon { width: 32px; height: 32px; background-size: contain; }",
			".folder { background-image: url('data:image/png;base64,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BAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECF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'); }",
			".file { background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QA/4ePzL8AAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAHdElNRQfjBhQENjszISZpAAABaUlEQVRIx+3VP0jUYRzH8ddzXup5KWoZDg3SGk1pW1BYUESbk0GuReDQVpNbQ0NIOLVkm6NDg0STi5JQgUNjpGEpRXDI+ee4x8Hs7n5y3e+i8d6f5QvP87z58jzwfIE21835al+5TgrmXVaHnEe+iw3zxaSu5OEguOeZDtveWrGDmNiTcc2oINg1a8pG7fJZH0VFD+XqteiSn6KyKFpwoXbxql+iD06pzzlr4u/biFbdFCrtdWhDwY6/Eauq8156oPNI0DzRGU890d+MICQUnSZNO51WUFJ2+GJHIbjjsfZsKsE3026r7O0ypE8w5gU3FESL8g0kOfk/6XPLpmjTcLoOoFhVb1u2ZaCZS0ySOVb8Ky1BS9AS/DdBUQm9DT+UWnp0Y0+RQe9Eu6b0pj4+4LmSaEF3wF0zTtqz5H2D6XBI3oiLsn6Y8BpOuO9ziuFanbJPxmUq//2ICVcMyh4brUmCfeveeGUVDgDBUY0iXJnNTQAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAxOS0wNi0yMFQwNDo1NDo1OS0wNDowMKG034gAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMTktMDYtMjBUMDQ6NTQ6NTktMDQ6MDDQ6Wc0AAAAAElFTkSuQmCC'); }",
			".download { transform: scale(0.7); background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAWlBMVEX///+AgIB7e3u9vb23t7fLy8uHh4f8/PyDg4PT09N9fX3Ozs7JycmLi4umpqZ5eXni4uLDw8PZ2dmsrKyWlpb29vbp6ent7e2YmJihoaGQkJDx8fFzc3OxsbG7ZtKBAAAGZUlEQVR4nO2d6WKiMBCAOZSQcAqi1t2+/2uuR2uLkDDmIBl3vv5sNfkayJBriKL1YKwt0zQtW8ZWLHU9hq6Pm+RKE/fd4Ls69mn7OIm/SeK+9V0h22zPP343x/PWd5XssstEPEZkO9+VskkRPwteFOPCd7XscZy04L0Vj74rZo2POcGL4ofvitmCZ7OCcZxx31WzRD/fhJdG7H1XzQ6Hk9TwdPBdOSvwSiIYx9V7XKZlLjXMS9+Vs0KaSA2T1HflrECG+CFD/JAhfsgQP2SIHzLEDxnihwzxQ4b4IUP8kCF+yBA/ZIgfMsQPGeKHDPFDhvghQ/yQIX7IED9kiB8yxA8Z4ocM8UOG+CFD/JAhfsgQP2SIHzLEDxnihwzxQ4b4IUP8kGGIHPkuTdNdB8tNYm547G7l8bVyoXT1vromEcirfd8B/t7UsOsf5dWQ8kw51rn4znMh4rz+s/gJM8M/df5I3yNEVjtvx+IpmY5IFv+tRoZd8lRe5jYzESvzSZ4SsZS9w8SQT4vLS5fZ3oqp4PW/qi5S35A9XzF3RYeteKhm8z1VakVtQ1ZIynOX1aaer+uCoq6hRPDyqdq22DetrKoXRcXHdA1lgpePuUrYV8tKvNwbiu5G05DP3PPfxTlqRCat6IVGrqhnyBtVcW66005VpEJRy1AtGLt5ttkpy4xzWXejYcgKeW6pG25yLm6k98UNaY/6uqG0F32UtXFi+KkuVar4suGiYCw+nRgutKE0aLxsuCjoqg0X7sNrwbOJEF815LOZMse4uQ/VfelXlWcUXzTk8j//wU1fyhb6txszQeM1w4UwcSd3NLyQP9P8ZtLdvGLICkgRrp5p5M+lo9Ing6kXDGeHS1MaZ4mkJWOLJ8XnoAE3XA4T9085G1tIxodLimBDoKDL8WHEARWYBA2wISRMXHGa9HQ7k7Z6RnE0dwM15AL03bHjVOdbWdbjMX9/KQIN+V/QN2fOc7mX0EvpcS+CDBn0Blgha+0WpPgraEAMgWFCuG/BmyLsv/3oUQGGwF40dn0PaiouG4YmGDGg4tcE/LIh7BK9CK72bhMGDBr3yLVoyIHftp5g9FrQWDIMJkw8K4IurGvoXzCcLr7MftHagtC4eL0X1YYF6GpYJQ4+A+1RuWJ+R2x4YL2ojuL5rPjt+RywIDRo2GDVXnSsCGsBM1YOE2O28vUhe4K51zdDAeOiCeuHiTHQwZQuXsLEGLf3ovMRPUzRJQEIulUMQtBd0PAaJsa4CRqew8QYJ0HDd5gYYz9oBBAmxti+UIO6RO/Y7lGDE7StGKCg3cFUMGFijK24GMSj2jyWgobTMGF4cdgIGsZhQurADh0v0oK3BwNN4CSjUtCgBdmh5cVuy7sZB9ZtTnnSXH6yU9rpO5p3N/qCrEtP2c0hP22eHdq+emz7F0n1qb9SbqqoL3j4HDmM3/xdjCf1hNhrr5UbBg39MMH341Vycf61y27mrdnKndtLirr3oslwabpA96vLmj9Fob/jQTdomHQyc/s4Hicz2tl5Z7E3uBf1FA0ED/tZh/PtXhz6+ZWTZKPfo+rERZM4yDYSh36IFJsqM4PdYzrdjUEcbKUXTXfRlzThdenLJPSvKcikC3hJz6JBumtUVCbPcK8qmjyLMvkSXT5ErXzHamNymvGloGE4q3ZUOLRRKV+hVZyDgQCf2DCdslDsK05K5Rq04X5xcNAwHS7tlOvoLs+SQ9f6TWfV1A6q3xqfaoBcqBZm1STRcA1DUI9qPqL3aQhQtDBl4dVwIWjYWXzxaqiegbM0q+bZUBU0LM2q+TaUBg1riy/eDSVBw97ii39DSY9qbeI3AMOZ7sbm1H0Ihoxn43KSjNtbfAnBMIoOH81PM4rmw+bZpTAMo6itq/toO69qu8frQjGMooHvNpvNjg+WvzccQ1eQIRmGDxmSYfiQIRmGDxmSYfiQIRmGDxmSYfiQIRmGDxmSYfiQ4X9sKE4bHJzkeyHUhrFIcKDYz7Jg+AaQIX7IED9kiB8yxA8Z4ocM8fNfGCrOPb0DSak6u/YONG00+K6DY4aIwd5agRRRs+s7FH1XwyG3908OoLdW4CSpb5t1W2CiW3yI6mu3tXnKlTD5ydHApnkx3gGR7R4HHti2er97MalGB//aWjWtihCRPJ94GLo6bnzPXVujietueuSBDW2ZvgdlO/xcoP8A3AB1jF6p0LcAAAAASUVORK5CYII='); }",
			"function download_file(path,name){return fetch(path).then(res=>res.blob()).then(blob=>{let a=URL.createObjectURL(blob);let dw=document.createElement('a');dw.href=a;dw.setAttribute('download',name);return dw;}).then(item=>item.click());}"
		);

		public static String getPath(String relPath, String path){
			if(relPath.Substring(relPath.Length-1) == "/")
				return String.Format("{0}{1}",relPath,path);
			else		
				return String.Format("{0}/{1}",relPath,path);
		}

		public static void Main(String[] args){
			// Get requested path
			String PATH = Environment.GetEnvironmentVariable("PATH_TRANSLATED");
			// Get the folder that contains the PATH
			String CURRENT_PATH = Environment.GetEnvironmentVariable("PATH_INFO");
			// Split into tokens of subdirectories
			String[] subdir = Regex.Split(PATH,"\\\\");
			// Get current file or folder name
			String filename = subdir[subdir.Length-1];

			// Check if it's either file or directory
			if(File.Exists(PATH) || Directory.Exists(PATH)){
				// Check if it's a file
				if(File.Exists(PATH)){
					// Get the file extension
					String extension = Regex.Split(filename,"\\.")[1];
					// Store content type
					String contentType = "";
					switch(extension){
						case "html": contentType = "text/html"; break;
						case "js": contentType = "text/javascript"; break;
						case "txt": contentType = "text/plain"; break;
					}
					// Check for another extensions if it's still not yet set
					if(contentType == "") contentType = GetExtension(extension);
					// Create the response
					response = new HttpResponse(200,"OK",contentType);
					// Write current file to the response buffer
					response.AppendBody(File.ReadAllText(PATH));
					// Write response
					Console.Write(response.ToString()); 
					return;
				}

				/* START DIRECTORY LISTING */

				// Initialize body
				String bodyBuffer = "";
				// Check if it's a folder
				if(Directory.Exists(PATH)){
					// Initialize folders and files counter
					int counter = 0;
					// Go through all directories and display
					foreach(String file in Directory.GetDirectories(PATH)){
						// Split into subdirectories
						String[] paths = Regex.Split(file,"\\\\");
						// Extract the current folder name
						String name = paths[paths.Length-1];
						// Exclude specific folders
						if(name == "bin" ||
							name == ".git") continue;
						// Increment folder and file counter
						counter++;
						// Append onto the response body
						bodyBuffer += String.Format(DIRECTORY_FORMAT,name,getPath(CURRENT_PATH,name));
					}
					// Go through all files and display
					foreach(String file in Directory.GetFiles(PATH)){
						// Split into subdirectories
						String[] paths = Regex.Split(file,"\\\\");
						// Extract the current file name
						String name = paths[paths.Length-1];
						// Extract the file extension
						String extension = Regex.Split(name,"\\.")[1];
						// Ignore specific extensions
						if(extension == "config" ||
							extension == "exe") continue;
						// Increment folder and file counter
						counter++;
						// Append onto the response body
						bodyBuffer += String.Format(FILE_FORMAT,name,getPath(CURRENT_PATH,name));
					}
					// Check current number of files and folders viewed
					if(counter == 0){
						// throw code 204 No Content, basically go back
						response = new HttpResponse(204,"No Content");
						Console.Write(response.ToString());
					}
				}

				// HttpResponse res = new HttpResponse(200,"OK","text/plain");
				HttpResponse res = new HttpResponse(200,"OK");
				HtmlDocument doc = new HtmlDocument();
				HtmlStyleElement style1 = new HtmlStyleElement();
				style1.SetAttribute("type","text/css");
				style1.SetTextContent(ConcatString(
					"div.content     { display: grid; grid-auto-flow: row; }",
					"div.content>div { display: grid; grid-gap: 20px; padding: 10px; grid-template-columns: 32px auto 32px; }",
					"div.content>div>* { display: flex; align-items: center; }",
					".icon { width: 32px; height: 32px; background-size: contain; }",
					".folder { background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAgAAAAIACAYAAAD0eNT6AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAALEwAACxMBAJqcGAAAGW9JREFUeJzt3XuMZmd92PHf87y3mdmdvfju1klDRPCuCUQVKeEiNZja+AJKKlWljWiTlkZK26gXaAuNDJ4dIDQIUVI1ahNRQmhCm1qt6lQNXtukNBJ1A4WGi/HaIkAdwMa39dre3bm973n6x17s9c7uzsy+l5l5Ph/JEvPO+57z+4c93znvOc+JAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABgc0mTHmA1r/qNL3aee/ypa9r92B2Rd5bczKZIO5umtCY9G1xIiqZJ0VpsoixGpGdTe/nRmSuueuRLv/DjK5OeDeCUiQfAK+fuvmIlleujpL8QKa5tSnNtpHhJjtye9GwwNKUpJeXvpVK+HpHuj2i+0Op2/vD+2254bNKjAXUafwDMzeXr4nVvaCL+ckrN9VHi5ZHyxEMEJuTBEvHfSyn/9aEDN/3vSKlMeiCgDmM78L78fffsb5rys02Jt+UUPzCu/cJWUSK+m0r5RLT7Hz/03rc8POl5gO1t5AFw7dxdb4iUb8sRN4x6X7BNNFHK7+Xc/Iuvz936fyY9DLA9jSwArps7eHNJ6b0R8bpR7QO2v+aenNO7vn77zV+Z9CTA9jL0AHjZ++9+Sasf/zpSvHnY24ZKNaXEJ/vdwbv/5LZbn5j0MMD2MLQAuG7ujm6Tdr87lfJLkdL0sLYLnFSapyLFOw4duOW3Jz0KsPUNJQCumzv40kHEHTmlPz+M7QHn1kTc2e2tvP1rv/SWpyc9C7B1XXQA7J+7662R8sciYtcQ5gHWoInmT3PKbz00d9PnJz0LsDVtPADm5vK+9NqPpkj/cIjzAGtVYqmJ8vaH5m/+D5MeBdh6NhQA183d0W1i1++klP7qsAcC1qdE3P7ggZveP+k5gK1l3QFw7YfunE0LvTtT5DeOYiBg/UopH35w/uZ3TXoOYOtYVwC8au6/zRyP3mcixWtHNRCwQaX56KH5W9456TGArSGv9Y0/OffZ9rHo/GcHf9ikUn7H/gMHb5v0GMDWsLYAKCU9lpY+kVK+ZcTzABclfeC6Awf/9qSnADa/1lretC+9ds7V/rA1NJFuufz6t/3Bk//zU9+d9CzA5nXBawCum7v7jSXFvbGOrwuAyWoivt8r8WNfnb/p8UnPAmxO5z2o/+gvf+bKQYpPXeh9wOaSI65aSuUTk54D2LzOe2AfLPc/mSOuGtcwwPDkSLfuO3D3L056DmBzOudXAPsO3PMzKYoVxmALa6I52i1l/9fmb3U9AHCGVc8AvHTu07tSKR8Z9zDAcOXIO1ei9WuTngPYfFY9A7Bv7uCvppT+0biHAYAtrTSlSXkll2YhUn46onkySvpOpPh2KflQjuarx6L31Yfnr1+c9KhnBcC1c3f9UKT4Ro7cnsRAALCdNRHLucSXSiqfTakcPLT/ufvirW8djHuOswJg39zd/yal+HvjHgQAalSieTxK/i8R6d8/OP+mPxrXfs8IgH0fuOvqtJK/HSl64xoAADiluT9F/rVjpfvJUX9NcOZFgP38Tgd/AJiU/KMl4ten0+LD++cO/tNrPnLf9Kj2dPoMwKt+44ud448++UhEumxUOwMA1qHEoymV2x6Yu+m3IqUyzE2fDoD983f9dJR85zA3DgBcvBLxhZLTzz90+5u+NqxtPv8VQMk/O6yNAgDDkyJeHYPBl/YdODgXd9yxpgf5rWGbET/8K/fu7iw2j+eI7jA2CgCMzH3tMvhrF7vCZ46I6C0MrnfwB4At4XX9aP3fa+fuesPFbCRHRJQU1w9lJABg9FJcHhH37Dtw989tdBP5xHbKG4c2FAAwcjnlTor4rf1zd79rQ59/2dxnL4sSLx/2YADAGKT40L4DB+fW+7Gc8tIrIuVzPhYYANjcUqQD+w4cfPd6PpNzU64d1UAAwHikSL+ynmsCcqQsAABgGyil+dha7w7IpTQCAAC2gZxyJ0e+4xVzn77mgu8tKa4ex1AAwBikuLwf6XcvtGJgTiXPjmsmAGAMUn79vq/vuu18b8mRGgEAANtMifKea993zyvO9ftcIu8c50AAwOjllDupKf8uSln1Vv+cI2bGPRQAMHop4tXXHrhn1VsD82ovAgDbQ4744DUfuW96ldcBgG0rxdWzzz77iy9+WQAAwDZXUvlnf27us1MvfE0AAMA2lyJfMZ2W/uYLXxMAAFCBVOIfvPBnAQAANUjpFfvn7/6JUz8KAACoRVNOfw0gAACgEk1KfyXm5nKEAACAauSIq/bl1/zEyf8NANQiNemWCAEAAJUpb4wQAABQmfzjL/1Xn+4JAACoSYpe70i8UgAAQGWa0hIAAFCbErFPAABAZVKJlwgAAKhMSfEDAgAAKpOiuUwAAEBlmohLBAAAVCZHTLUnPQQwOe2pnZMeAbafUqJEidIMojRNlEE/IsqkpzpTiZ4AgFqlFL09V0x6CqhCaQbR9JejWVmOZmUpBsuLUZr+5AZKOQkAqFRKrUmPANVIuRWt7nS0utOnXyuDlRgsHY+VxePRLC/GuM8SCACoVEpp0iNA1VKrE+2Z3dGe2R2lGUR/4Wj0F56Npr8ylv0LAKhVdg0wbBYpt6KzY3d0duyOwdJCrBw/EoOlhZHuUwBApXL2FQBsRq3edLR609GsLMXyc4djsDyaEPAnAFQqtbuTHgE4j9zpxdQlV8fU3qsitzrD3/7QtwhsCaP4BwUYvlZvJqYvvyY6O/cMdbsCACqV2wIAto4U3Z2XxPSlf2Zo8S4AoEopsq8AYMvJnamYvuyaoSziJQCgQq1O110AsFWdXMSrO3vpRW3GvwBQodydmvQIwEXq7NgdU3uujIiNrekhAKBCL1yNDNi6WlM7YuqSqyI2sLCXAIDapCQAYBtpdadjau/VEWl9h3QBAJVpT+3Y0F8LwObV6k6d/Dpg7QQAVKY9PTvpEYARaPWmo7f78jW/XwBARU48kcwFgLBdtadn17xgkACAinRmdsVGrxgGtobOzr3R6l34Oh8BALVIKdo7dk96CmDEUqTo7b4i0gUe+CUAoBKdmd2R1nmVMLA1pdyK7gWuB/CvAVQgpRSdHbsmPQYwRu3ezHmXDBYAUIH2jt2RcnvSYwBj1tt16TnP/AkA2OZyqx2dnXsnPQYwCbl1zrsCBABsc91dl0Vy5T9UqzOz+hlAAQDbWGtqR7R6M5MeA5iklFY9CyAAYJvKrXZM7Vr7qmDA9tWZnj3rtkABANtUb8+VEdn/xYFYdR0Q/zrANtSdvTRypzfpMYBNpPui54AIANhmOjO7o2PFP+DFcitaUzue/3GCowBD1p7aGd1dl0x6DGCTeuHCQAIAtolW99SjQN3yB6yu1ZuJSCf+jRAAsA20ejuit/eq0//HBlhNSila3RO3BgsA2OLa07PR23tFJAd/YA3aJx8VbHFw2MK6O/da5hdYl5YAgK0r5Ry93VdY5Q9Yt9TqRMpZAMBWkztT0dt7ZeQXreoFsFa53RMAsFWklKM7uzdaM7s83Ae4KLnTFQCwFZy6v3+1J3oBrFdqdQQAbGat3kx0duyJVndq0qMA20hutQUAbEbt3o7ozO6J3LaePzB8SQDA5pFbnWhPz0Z7Zkek3Jn0OMB2llsCACYlRYrU6Ua7Ox2tqR2e3geMTUpJAMB4pMjtTqR2J3KrG7nbi1Z3KlKyGCcwfik2WQCkSJHa3UjtduRWO1LOEal1YonTU//BJpdKRKQcKZ/4r6QcOefwkB5g05j0GYDc7kTuTke7Ox2p04vc2lQ9AkPhsA9sRmM/4uZO78SFTr2ZSA74ADARYzkCp9yKzvRstKZnI7dd3QwAkzbSAMi5Fe2de6MzvTPCxU4AsGmMJABSbkV3595oT8+6cA8ANqGhB0B7eja6s5eeuIIfANiUhhYAud2N3u7LLWYCAFvAUAKgPTMb3V2XeUQpAGwRFxcAKUVv92XRnpod0jgAwDhsOABSbsXUJVd5WhkAbEEbCoDc6kTvkqut3AcAW9S6j+C504vpvVdF5NYo5gEAxmBdAZBanZjae3WEW/wAYEtb85E8tdoxfenV7u8HgG1gbUfz1IqpvVdFyr7zB4DtYE0BMLXn8sjt7qhnAQDG5IIB0NmxJ1q9mXHMAgCMyXkDoNWZiu7sJeOaBQAYk3MHQMrR3XPFGEcBAMblnAHQ2bnHQj8AsE2tGgC53YnuzO5xzwIAjMmqAdDddVlE8mQ/ANiuzgqAVncqWt3pScwCAIzJWQHQ2bl3EnMAAGN0RgDkjr/+AaAGZwRAd4cL/wCgBqcDIOVsxT8AqMTpAGhP7XTlPwBU4vkAmJ6d5BwAwBjliIjcakfu9CY9CwAwJicCwJX/AFCVHBFu/QOAypwIgJ4AAICa5NTqRMqtSc8BAIxRbrU7k54BABiznNrdSc8AAIxZzi1nAACgNjn5CgAAqpNTPuuJwADANpdTtv4/ANQmR7gFEABq4ysAAKiQoz8AVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAECFBAAAVEgAAEBtSokcpUx6DABgjEqUyKU0k54DABinpolcmv6kxwAAxqicCABnAACgJqXpRy4DZwAAoCbNoB+5WVme9BwAwBiV/nLkZiAAAKAmTX8lcrO8NOk5AIAxGqwsn7gNsBmsTHoWAGAMmv5yRBmcWAlwsLQw6XkAgDEYLC9GxMmlgBsBAABVGCwdj4iTATBYXohiSWAA2N5Kc+YZgFKaaE4WAQCwPQ2WFiJOPgLg9NMAVxaPTmwgAGD0+gvPH+tPB8Bg6XiUZjCRgQCA0SrNIPpLx07/nJ//TYmVhecmMRMAMGL9hWfP+Dmf8ctjz0S4GBAAtpkSK8fOEwClGTgLAADbzMrx5876mj+f9aajRyLCWQAA2A5KKdE/+vRZr58VAKXpn3WaAADYmlaOPxPNKhf554hoznrz0afdEQAAW1xpBrGyyl//UWKQI5qzngdcShPLzz41jtkAgBFZevbJVS/ub1Is5ShxbJXPRH/xaAyWVv0VALDJ9ZeOxWBx9eN4LnEsR0rn/FN/6ZknozT9kQ0HAAxfaQaxfOSJc/8+midzU9Jj59vA4pEnorgrAAC2hBIllo48HqWcdYnfaSnlx3JK5eHzbahZXoiV51a5gAAA2HRWnns6BssL539TaR7OqaRvXnBjx45E3wJBALCp9Y8/FyvHjlzwfSXlb+YUcf9aNrr0zBMx8MhgANiUBkvHY+nZc3/vf4YU9+eI8uW1bnzp6ccufFoBABirwfJCLB055yV9Z7+/yV9JERH7Dxx8IiJdtqZPpRxTe6+MVnd6Y1MCAEMzWF6IpcPfX/MF+03E9x86cNPVOSKilPS/1ryn0sTi4e9Hf/HoxiYFAIaiv3gsFp9e+8E/IiJF87mI558FcM/6dnniFoPlNVxoAAAM38qxp0+c9l9lpb/zS/dERLQjIko0n05nPxfowjt/7nCU5aXo7rk8Ulr/5wGA9SmlOXFh/jlW+buQTmnuiohIp17YP3f3FyPFqzaysdxqR2/PlZE7vQ0NAwBc2GB5MZafeTyawcZW6W1K8/mH5m95TcQZjwMuv7vRgZpBPxafeiSWn3vKqoEAMGylieXnDsfS4Uc3fPCPiEiRTh/rTwdAq9v+7aY0KxueLUqsHHsmFp/4TvQ9RAgAhmKweCyOP/G9WDl25KL+yG4ilvvd5lOnfj4dAPffdsNjKcXvXeSc0Qz6sfT0Y7F4+JFoVhYvdnMAUKXB8kIsPvVILB55LC7i7/PTUsSdf3LbradXCjrjyr1S0kcveg8nDZYXY+GpR2Lx8KNWEASANRosHY/FwyePn0P8QzqX8pEX/pxe/Ib9c3d9LlJ+/dD2eGpHrU50pmejPb0zUqs97M0DwJZVmn70jx+N/sKzF/Ud/7l3UP7w0PzNb3jhS2cdiVPk20vEHwx934OVWD56OJaPHo7cnYp2byZavZnI7e6wdwUAm17TX47B0kIMlo7FYHnUX5mn2896ZbW3XXvg7ntzxA0jnubEALkVre5U5E4vcrsbqd2N7AwBANtIM+hH6S9Hs7IczcpS9FcWI5rBWPZdIg4+eOCmW178+qpH2lTiHRHx5UjRGvlgzSD6i8ciXrSgQcrtSDlHyq2IlCKl5AZDADa1FBGllIhSojSDKKWJGAwmdot8E02/nVvvXO13q54BiIjYf+DgRyLSqh8CADa/UsqHH5y/+V2r/e6c6/fOlOX3RjTfGt1YAMColBLfPLpr19y5fn/OAPjS/E8dT6X9c1FiPF9SAABD0UTTj0h/47v/5HUL53rPeZ/g88D8jZ+LiA8MfTIAYGRyyfMPzr/pj877ngtt5FDc974ScXB4YwEAI1Pi9w8deNMvX+htF36G7/x80yvdnymlOTSUwQCAkWhKfH1pOr8tUrrgbQcXDoCI+Mr89UdKxK1NxPcvfjwAYOhKPJrbK2/+1j+/8Zm1vH1NARAR8dD8Lf8vlbgxSvPUxqcDAIauNE+lGNxw6L1veXitH1lzAEREPDh/0/3Rat8YUZ5c/3QAwNCVeCJFvuGB+VsfWM/H1hUAERGHbr/xj1NpfjJK+d56PwsADE9T4juDKH/xgfmbvrzez55zJcAL+ZEPfObPtlYGv59S/NhGtwEAbExTyh+nTnnzg++55dGNfH7dZwBO+cZ7bvjejlh6XZTyHze6DQBg/UqU3zm2a9frN3rwj7iIMwAvtH/urr8bkf5lpDQ9jO0BAGdrIo6nKP/4wQM3f+xitzWUAIiIePn77tnfNM1vRqTXDGubAMBp9zW5eftDt9/y0DA2NrQAiIiIubm8L17790uk9+UUe4e6bQCoUBNxOEd5z6G5m359LQv8rNVwA+CkfR+899JYauZKil/IEd1R7AMAtrUSS5Hi36ZS3v/A/M2Hh735kQTAKdfN3fuDJQ3e3UT6WzliZpT7AoDtoCnNsRT5Nwed1oe+8Z4bRnbL/UgD4JR9H7z30rTU/J0mmp/PKf/IOPYJAFtKiYcixcdTKR8fxV/8LzaWAHihfXP3vCai+esppZ+OiB8a9/4BYNMozbcj4s7I+T8dmrvp8+Pc9dgD4IVeNndwX07pL6VSXl8ivTpF88OR8kRnAoARaZqIb+WIL0SUzzW5/I9hXdG/EZvqYPvKD9+9Y/loc23J6SWppB+MKJdFpEtSlKko0WsibXjhIgAYtRyliRRLJdJiRDkcJZ6IKH+aWu1vzwwWHvrS/E8dn/SMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACw7fx/J+NYdI+0Jm4AAAAASUVORK5CYII='); }",
					".file { background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QA/4ePzL8AAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAHdElNRQfjBhQENjszISZpAAABaUlEQVRIx+3VP0jUYRzH8ddzXup5KWoZDg3SGk1pW1BYUESbk0GuReDQVpNbQ0NIOLVkm6NDg0STi5JQgUNjpGEpRXDI+ee4x8Hs7n5y3e+i8d6f5QvP87z58jzwfIE21835al+5TgrmXVaHnEe+iw3zxaSu5OEguOeZDtveWrGDmNiTcc2oINg1a8pG7fJZH0VFD+XqteiSn6KyKFpwoXbxql+iD06pzzlr4u/biFbdFCrtdWhDwY6/Eauq8156oPNI0DzRGU890d+MICQUnSZNO51WUFJ2+GJHIbjjsfZsKsE3026r7O0ypE8w5gU3FESL8g0kOfk/6XPLpmjTcLoOoFhVb1u2ZaCZS0ySOVb8Ky1BS9AS/DdBUQm9DT+UWnp0Y0+RQe9Eu6b0pj4+4LmSaEF3wF0zTtqz5H2D6XBI3oiLsn6Y8BpOuO9ziuFanbJPxmUq//2ICVcMyh4brUmCfeveeGUVDgDBUY0iXJnNTQAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAxOS0wNi0yMFQwNDo1NDo1OS0wNDowMKG034gAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMTktMDYtMjBUMDQ6NTQ6NTktMDQ6MDDQ6Wc0AAAAAElFTkSuQmCC'); }",
					".download { transform: scale(0.7); background-image: url('data:image/png;base64,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'); }"
				));
				doc.GetHead().AppendChild(style1);
				HtmlScriptElement script1 = new HtmlScriptElement();
				script1.SetAttribute("type","text/javascript");
				script1.SetTextContent("function download_file(path,name){return fetch(path).then(res=>res.blob()).then(blob=>{let a=URL.createObjectURL(blob);let dw=document.createElement('a');dw.href=a;dw.setAttribute('download',name);return dw;}).then(item=>item.click());}");
				doc.GetHead().AppendChild(script1);
				HtmlDivElement div1 = new HtmlDivElement();
				div1.SetAttribute("class","content");
				div1.SetTextContent(bodyBuffer);
				doc.GetBody().AppendChild(div1);
				res.SetBody(doc.ToString());
				Console.Write(res.ToString());

				/* END DIRECTORY LISTING */
			}else /* If not exists, throw error 404 */ {
				response = new HttpResponse(404,"NOT FOUND");
				response.AppendBody("Opss! Not found.");
				Console.Write(response.ToString());
			}
		}

	}

}