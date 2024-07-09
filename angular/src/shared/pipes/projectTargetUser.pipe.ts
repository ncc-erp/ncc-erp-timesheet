import { Pipe, PipeTransform } from '@angular/core';
import {IProjectTargetUser} from '@app/modules/branch-manager/modal/project-management-modal/interface/project-type.interface';
import {ValueTypeTargetUserMap} from '@app/modules/branch-manager/modal/project-management-modal/constant/type-target-user.constant';

@Pipe({
    name: 'projectTargetUser'
})

export class ProjectTargetUserPipe implements PipeTransform {
    transform(value: IProjectTargetUser[]): IProjectTargetUser[] {
        const totalWorkingTime = value.reduce((acc, user) => acc + Number(user.workingTime), 0);
        return value.map(user => {
            const percentage = (Number(user.workingTime) / totalWorkingTime) * 100;
            const valueTypeTargetUser = ValueTypeTargetUserMap[user.valueType];
            const workingTimePercentage = percentage === 100 ? '100%' : `${percentage.toFixed(2)}%`;
            return {
                ...user,
                workingTime: workingTimePercentage,
                valueType: valueTypeTargetUser
            };
        });
    }
}
