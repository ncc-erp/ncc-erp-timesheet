import { Pipe, PipeTransform } from '@angular/core';
import { projectListManagement } from '@app/modules/branch-manager/Dto/branch-manage-dto';

@Pipe({
  name: 'showProjectInActive'
})
export class ShowProjectInActivePipe implements PipeTransform {

  transform(users: projectListManagement[], value: boolean,): any {
    if (value) {
      return users;
    }
    return users.filter((i) => i.status === 0);
  }

}
