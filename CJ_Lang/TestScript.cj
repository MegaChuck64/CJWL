//jump
//label
//var 
// ==
// !=
// >
// <
// >=
// <=
// =
// +
// -
// /
// *
// %
// string literal
// int literal
// float literal
// bool literal


App:
	var a = 1
	var b = 2

	jump ConditionIsTrue (a == b)
	jump ConditionIsFalse (a != b)
	
	ConditionIsFalse:
		print("a is equal to b")
		jump End

	ConditionIsTrue:
		print("a is equal to b")
		jump End


	End:
	print("end")

