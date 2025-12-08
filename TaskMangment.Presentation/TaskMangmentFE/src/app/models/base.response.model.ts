export interface BaseResponse<T>{
    data: T;
    successMessage: string;
    statusCode: Number;
    errorList: string[];
}