const request = new XMLHttpRequest();
request.open("GET", "output.wasm");
request.responseType = "arraybuffer";
request.send();

request.onload = () => {
  const bytes = request.response;
  WebAssembly.instantiate(bytes).then((results) => 
  {

    const { addTwoIfZero } = results.instance.exports;

    Test_AddTwoIfZero(addTwoIfZero);

    const { isGreaterThan } = results.instance.exports;

    Test_IsGreaterThan(isGreaterThan);

    const { subTwoIfPositive } = results.instance.exports;

    Test_SubTwoIfPositive(subTwoIfPositive);

    const { isNegative } = results.instance.exports;

    Test_IsNegative(isNegative);    
        
    // const { mulIfLessThan } = results.instance.exports;

    // Test_MulIfLessThan(mulIfLessThan);

    // const { isEven } = results.instance.exports;

    // Test_IsEven(isEven);

  });
};

function Test_IsEven(isEven)
{
    console.log("Test_IsEven");
    let pass = isEven(2);
    console.log("expected: 1 \nrecieved: " + pass);

    let fail = isEven(3);
    console.log("expected: 0 \nrecieved: " + fail);

    console.log("Test_IsEven complete");
    //draw line seperator
    console.log("--------------------------------------------------");
}


function Test_MulIfLessThan(mulIfLessThan)
{
    console.log("Test_MulIfLessThan");
    let pass = mulIfLessThan(3, 4, 13);
    console.log("expected: 12 \nrecieved: " + pass);

    let fail = mulIfLessThan(3, 4, 2);
    console.log("expected: 0 \nrecieved: " + fail);

    console.log("Test_MulIfLessThan complete");
    //draw line seperator
    console.log("--------------------------------------------------");
}

function Test_IsNegative(isNegative)
{
    console.log("Test_IsNegative");
    let pass = isNegative(-1);
    console.log("expected: 1 \nrecieved: " + pass);

    let fail = isNegative(1);
    console.log("expected: 0 \nrecieved: " + fail);

    console.log("Test_IsNegative complete");
    //draw line seperator
    console.log("--------------------------------------------------");    
}

function Test_SubTwoIfPositive(subTwoIfPositive)
{
    console.log("Test_SubTwoIfPositive");
    let pass = subTwoIfPositive(100, 50);
    console.log("expected: 50 \nrecieved: " + pass);

    let fail = subTwoIfPositive(50, 100);
    console.log("expected: 0 \nrecieved: " + fail);

    console.log("Test_SubTwoIfPositive complete");
    //draw line seperator
    console.log("--------------------------------------------------");    
}

function Test_IsGreaterThan(isGreaterThan)
{
    console.log("Test_IsGreaterThan");
    let pass = isGreaterThan(1, 3);
    console.log("expected: 0 \nrecieved: " + pass);

    let fail = isGreaterThan(3, 1);
    console.log("expected: 1 \nrecieved: " + fail);

    console.log("Test_IsGreaterThan complete");
    //draw line seperator
    console.log("--------------------------------------------------");    
}

function Test_AddTwoIfZero(addTwoIfZero)
{
    console.log("Test_AddTwoIfZero");
    let pass = addTwoIfZero(1, 3, 0);
    console.log("expected: 4 \nrecieved: " + pass);

    let fail = addTwoIfZero(1, 3, 1);
    console.log("expected: 0 \nrecieved: " + fail);

    console.log("Test_AddTwoIfZero complete");
    //draw line seperator
    console.log("--------------------------------------------------");    
}