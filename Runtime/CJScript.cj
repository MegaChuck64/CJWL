﻿app TestApp:
	entry (string[] args):
		print("Hello World")

		ArgPrinter printer = ArgPrinter(args)
		printer.PrintArgs()
		
	
	class ArgPrinter(string[] args):
	  	string[] Args = args 
		void PrintArgs():
			print(FormatArgs("-"))

		string FormatArgs(string seperator):
			string result
			foreach arg in Args:
				result += arg + seperator
			
			return result
