import { Pipe, PipeTransform } from '@angular/core';
import { projectList } from '@app/modules/branch-manager/manage-employee/detail-participating-projects/detail-participating-projects.component';

@Pipe({
  name: 'showProjectInActive'
})
export class ShowProjectInActivePipe implements PipeTransform {

  transform(users: projectList[], value: boolean, ): any {
    if (value) {
      return users;
    }
    return users.filter((i) => i.status === 0);
  }

}
