import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'decimalRounding'
})
export class DecimalRoundingPipe implements PipeTransform {

  transform(value: number, precision: number): unknown {
    return value.toFixed(precision).replace(/\.00$/, '')
  }

}
