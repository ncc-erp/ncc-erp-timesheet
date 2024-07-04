import { Pipe, PipeTransform } from '@angular/core';
import {
    ETypeTargetUser
} from '@app/modules/branch-manager/modal/project-management-modal/emum/type-target-user.enum';
import {IProjectTargetUser} from '@app/modules/branch-manager/modal/project-management-modal/interface/project-type.interface';

@Pipe({
    name: 'projectTargetUser'
})

export class ProjectTargetUserPipe implements PipeTransform {
    transform(value: IProjectTargetUser[]): IProjectTargetUser[] {
        const totalWorkingTime = value.reduce((acc, user) => acc + Number(user.workingTime), 0);
        return value.map(user => {
            const percentage = (Number(user.workingTime) / totalWorkingTime) * 100;
            let valueTypeTargetUser = '';

            if (user.valueType === ETypeTargetUser.Member) {
                valueTypeTargetUser = 'Member';
            } else if (user.valueType ===  ETypeTargetUser.Expose) {
                valueTypeTargetUser = 'Expose';
            } else {
                valueTypeTargetUser = 'Shadow';
            }
            const workingTimePercentage = percentage === 100 ? '100%' : `${percentage.toFixed(2)}%`;
            return {
                ...user,
                workingTime: workingTimePercentage,
                valueType: valueTypeTargetUser
            };
        });
    }
}
