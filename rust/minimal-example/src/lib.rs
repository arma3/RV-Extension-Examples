use std::os::raw::c_char;
use std::ffi::{CString, CStr};
use std::str;
use std::cmp;

type size_t = usize;

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn RVExtensionVersion(output_ptr: *mut i8, output_size: size_t) {
    unsafe { write_str_to_ptr("Test Extension v.1.00", output_ptr, output_size) };
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn RVExtension(
    //Use a u8 here to make copying into the response easier. Fundamentally the same as a c_char.
    response_ptr: *mut c_char,
    response_size: size_t,
    request_ptr: *const c_char,
) {
    // get str from arma
    let request: &str = {unsafe { CStr::from_ptr(request_ptr) }}.to_str().unwrap();

    // send str to arma
    unsafe { write_str_to_ptr(request, response_ptr, response_size) };
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn RVExtensionArgs(
    response_ptr: *mut c_char,
    response_size: size_t,
    function_name_ptr: *const c_char,
    args_ptr: *const *const c_char,
    argsCount: i32
) {
    let function_name: &str = {unsafe { CStr::from_ptr(function_name_ptr) }}.to_str().unwrap();
    let mapped_args = unsafe {
        //This is a safe cast, as long as argsCount isn't negative, which it never should be. Even so, debug_assert it.
        debug_assert!(argsCount >= 0);
        let arg_ptrs = std::slice::from_raw_parts(args_ptr, argsCount as usize);
        arg_ptrs.iter()
                .map(|&ptr| CStr::from_ptr(ptr))
                .map(|cstr| cstr.to_str())
    };

    //Args here 
    let args: Vec<&str> = match(mapped_args.collect()) {
        Ok(args) => args,
        Err(_) => return
    };

    //Handle here
    let response_message = "Test Response";
    unsafe { write_str_to_ptr(response_message, response_ptr, response_size) };
}

type ArmaCallback = extern fn(*const c_char, *const c_char, *const c_char) -> i32;

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn RVExtensionRegisterCallback(
    callback: ArmaCallback
) {
    let name = "Test Extension";
    let func = "Test Function";
    let data = "Test Data";

    call_extension_callback(callback, name, func, data);
}

/// A safer-way to call an extension callback.
/// Verifies input is ASCII, and turns &str into null-byte terminated C strings.
pub fn call_extension_callback(callback: ArmaCallback, name: &str, func: &str, data: &str) -> Option<()> {
    if !(name.is_ascii() && func.is_ascii() && data.is_ascii()) {return None};

    //Verify we created all of the CStrings successfully.
    let cstr_name = CString::new(name).ok()?;
    let cstr_func = CString::new(func).ok()?;
    let cstr_data = CString::new(data).ok()?;

    //into_raw() releases ownership of them. Arma becomes responsible for cleaning the strings up.
    callback(cstr_name.into_raw(), cstr_func.into_raw(), cstr_data.into_raw());
    Some(())
}

/// Copies an ASCII rust string into a memory buffer as a C string.
/// Performs necessary validation, including:
/// * Ensuring the string is ASCII
/// * Ensuring the string has no null bytes except at the end
/// * Making sure string length doesn't exceed the buffer.
/// # Returns
/// :Option with the number of ASCII characters written - *excludes the C null terminator*
unsafe fn write_str_to_ptr(string: &str, ptr: *mut c_char, buf_size: size_t) -> Option<usize> {
    //We shouldn't encode non-ascii string as C strings, things will get weird. Better to abort, I think.
    if !string.is_ascii() {return None};
    //This should never fail, honestly - we'd have to have manually added null bytes or something.
    let cstr = CString::new(string).ok()?;
    let cstr_bytes = cstr.as_bytes();
    //C Strings end in null bytes. We want to make sure we always write a valid string.
    //So we want to be able to always write a null byte at the end.
    let amount_to_copy = cmp::min(cstr_bytes.len(), buf_size - 1);
    //We provide a guarantee to our unsafe code, that we'll never pass anything too large. 
    //In reality, I can't see this ever happening.
    if amount_to_copy > isize::MAX as usize {return None}
    //We'll never copy the whole string here - it will always be missing the null byte.
    ptr.copy_from(cstr.as_ptr(), amount_to_copy);
    //strncpy(ptr, cstr.as_ptr(), amount_to_copy);
    //Add our null byte at the end
    ptr.add(amount_to_copy).write(0x00);
    Some(amount_to_copy)
}