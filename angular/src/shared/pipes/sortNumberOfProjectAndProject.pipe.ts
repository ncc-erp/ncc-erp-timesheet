import { Pipe, PipeTransform } from '@angular/core';
import { ManageUserDto } from '@app/modules/branch-manager/Dto/branch-manage-dto';
import { ESortProjectUserNumber, ESortType } from '@app/modules/branch-manager/manage-employee/enum/sort-project-user-number.enum';

@Pipe({
    name: 'sortProjectAndNumber'
})
export class SortNumberOfProjectAndProjectPipe implements PipeTransform {

    transform(users: ManageUserDto[], sortNumber: string, sortProject: number, sortType: ESortType): ManageUserDto[] {
        switch (sortType) {
            case ESortType.DEFAULT:
                return users;
            case ESortType.NUMBER:
                if (sortNumber === ESortProjectUserNumber.UP_NUMBER) {
                    return [...users].sort((a, b) => a.projectCount - b.projectCount);
                }
                break;
            case ESortType.PROJECT:
                if (sortProject === ESortProjectUserNumber.UP_PROJECT) {
                    return [...users].sort((a, b) => {
                        const firstProject = a.projectUsers[0];
                        const secondProject = b.projectUsers[0];

                        const workingTimeComparison = firstProject.workingTimePercent - secondProject.workingTimePercent;
                        if (workingTimeComparison !== 0) {
                            return workingTimeComparison;
                        }
                        return firstProject.projectName.localeCompare(secondProject.projectName);
                    });
                }
                break;
        }
    }
}
