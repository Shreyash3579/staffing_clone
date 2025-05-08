
interface Array<T> {
    isArrayEqual(array1: T[]): boolean;
}
  
Array.prototype.isArrayEqual = function<T>(array1: T[]): boolean {
    return JSON.stringify(array1) === JSON.stringify(this);
};