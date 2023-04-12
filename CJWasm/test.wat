(module
  (func 
  (export "addTwoPlusTen") 
  (param i32 i32) 
  (result i32) 
  (local i32 i64) 
    i32.const 10
    local.set 2
    i64.const 4300
    local.set 3
    local.get 0
    local.get 1
    i32.add
    local.get 2
    i32.add
  )
)
