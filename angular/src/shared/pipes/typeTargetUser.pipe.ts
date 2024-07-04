import { Pipe, PipeTransform } from '@angular/core';
import {
    ETypeTargetUser
} from '@app/modules/branch-manager/modal/project-management-modal/emum/type-target-user.enum';

@Pipe({
    name: 'typeTargetUser'
})

export class TypeTargetUserPipe implements PipeTransform {
    transform(value: number): string {
        if (value === ETypeTargetUser.Member) {
            return 'Member';
        }
        if (value === ETypeTargetUser.Expose) {
            return 'Expose';
        }
        return 'Shadow';
    }
}
