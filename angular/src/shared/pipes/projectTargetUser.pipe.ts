import {Pipe, PipeTransform} from '@angular/core';
import {IProjectTargetUser} from '@app/modules/branch-manager/modal/project-management-modal/interface/project-type.interface';
import {ValueTypeTargetUserMap} from '@app/modules/branch-manager/modal/project-management-modal/constant/type-target-user.constant';
import {ESortMemberEffort, ESortType} from '@app/modules/branch-manager/modal/project-management-modal/enum/sort-member-effort.enum';

@Pipe({
    name: 'projectTargetUser'
})

export class ProjectTargetUserPipe implements PipeTransform {
    transform(value: IProjectTargetUser[], sortMember: ESortMemberEffort, sortEffort: ESortMemberEffort,
              sortType: ESortType): IProjectTargetUser[] {
            if (sortType) {
                switch (sortMember) {
                    case ESortMemberEffort.DOWN_MEMBER:
                        value.sort((a, b) => b.fullName.localeCompare(a.fullName));
                        break;
                    case ESortMemberEffort.UP_MEMBER:
                        value.sort((a, b) => a.fullName.localeCompare(b.fullName));
                        break;
                }
            } else {
                switch (sortEffort) {
                    case ESortMemberEffort.DOWN_EFFORT:
                        value.sort((a, b) => this.getNumericWorkingTime(a) - this.getNumericWorkingTime(b));
                        break;
                    case ESortMemberEffort.UP_EFFORT:
                        value.sort((a, b) => this.getNumericWorkingTime(b) - this.getNumericWorkingTime(a));
                        break;
                }
            }

        return value.map(user => {
            const percentage = Number(user.workingPercent);
            const valueTypeTargetUser = ValueTypeTargetUserMap[user.valueType];
            const workingTimePercentage = percentage === 100 ? '100%' : `${percentage.toFixed(2)}%`;
            return {
                ...user,
                workingTime: workingTimePercentage,
                valueType: valueTypeTargetUser
            };
        });
    }

    private getNumericWorkingTime(user: IProjectTargetUser): number {
        return typeof user.workingPercent === 'string' ? Number(user.workingPercent) : user.workingPercent as number;
    }
}
