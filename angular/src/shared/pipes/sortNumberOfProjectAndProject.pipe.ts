import { Pipe, PipeTransform } from '@angular/core';
import {ManageUserDto} from '@app/modules/branch-manager/Dto/branch-manage-dto';
import {ESortProjectUserNumber} from '@app/modules/branch-manager/manage-employee/enum/sort-project-user-number.enum';

@Pipe({
    name: 'sortProjectAndNumber'
})
export class SortNumberOfProjectAndProjectPipe implements PipeTransform {

    transform(users: ManageUserDto[], sortNumber: string, sortProject: number): ManageUserDto[] {
        if (sortNumber === ESortProjectUserNumber.UP_NUMBER) {
            return [...users].sort((a, b) => a.projectCount - b.projectCount);
        } else if (sortProject === ESortProjectUserNumber.UP_PROJECT) {
            return [...users].sort((a, b) => {
                const firstProject = a.projectUsers[0];
                const secondProject = b.projectUsers[0];

                if (firstProject.workingTimePercent < secondProject.workingTimePercent) {
                    return -1;
                }
                if (firstProject.workingTimePercent > secondProject.workingTimePercent) {
                    return 1;
                }

                return firstProject.projectName.localeCompare(secondProject.projectName);
            });
        }
        return users;
    }
}
