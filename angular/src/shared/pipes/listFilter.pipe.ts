		
import { PipeTransform } from "@angular/core";
import { DropdownData } from "@app/modules/capabilities/capability.component";
export class ListFilterPipe implements PipeTransform {
    transform(list: any[], keyPropertyName: string, valuePropertyName: string):DropdownData[] {
        return list.map(item => ({
            key: item[keyPropertyName],
            value: item[valuePropertyName],
        })) 
    }
}