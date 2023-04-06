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
	b = a + b

	jump ConditionIsTrue (a == b)
	jump ConditionIsFalse (a != b)
	
	ConditionIsFalse:
		print("a is not equal to b")
		jump End

	ConditionIsTrue:
		print("a is equal to b")
		jump End


	End:
		print("end")

