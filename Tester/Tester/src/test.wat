(module
  (func (export "addTwoIfZero") 
    (param i32 i32 i32)
    (result i32)
    (local i32)
    
    i32.const 0
    local.set 3
    
    local.get 3
    local.get 2
    i32.eq
    if 
    local.get 0
    local.get 1
    i32.add
    local.set 3
    end
    
    local.get 3
  )
)

;; i32.const 0 (41, 00)
;; local.set 2 (21, 02)
;; local.get 0 (20, 00)
;; i32.const 5 (41, 05)
;; i32.gt_s (4a)
;; if (04)
;; void (40)
;; local.get 2 (20, 02)
;; local.get 0 (20, 00)
;; i32.add (6a)
;; local.set 2 (21, 02)
;; end (0b)
;; local.get 1 (20, 01)
;; i32.const 5 (41, 05)
;; i32.gt_s (4a)
;; if (04)
;; void (40)
;; local.get 2 (20, 02)
;; local.get 1 (20, 01)
;; i32.add (6a)
;; local.set 2 (21, 02)
;; end (0b)
;; local.get 2 (20, 02)



		;; (local.set 2 (i32.const 0))

		;; ;; if (left > 5)
		;; (if
		;; 	(i32.gt_s (local.get 0) (i32.const 5))
		;; 	(then
		;; 		;; res = res + left
		;; 		(local.set 2 (i32.add (local.get 2) (local.get 0)))
		;; 	)
		;; )

		;; ;; if (right > 5)
		;; (if
		;; 	(i32.gt_s (local.get 1) (i32.const 5))
		;; 	(then
		;; 		;; res = res + right
		;; 		(local.set 2 (i32.add (local.get 2) (local.get 1)))
		;; 	)
		;; )

		;; ;; return res
		;; (local.get 2)