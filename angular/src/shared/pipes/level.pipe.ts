import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'level'
})
export class LevelPipe implements PipeTransform {

  transform(level:any): any {
    if (level == 0) {
      return "Intern_0 "
    }
    if (level == 1) {
      return "Intern_1"
    }
    if (level == 2) {
      return "Intern_2"
    }
    if (level == 3) {
      return "Intern_3"
    }
    if (level == 4) {
      return "Fresher-"
    }
    if (level == 5) {
      return "Fresher"
    }
    if (level == 6) {
      return "Fresher+"
    }
    if (level == 7) {
      return "Junior-"
    }
    if (level == 8) {
      return "Junior"
    }
    if (level == 9) {
      return "Junior+"
    }
    if (level == 10) {
      return "Middle-"
    }
    if (level == 11) {
      return "Middle"
    }
    if (level == 12) {
      return "Middle+"
    }
    if (level == 13) {
      return "Senior-"
    }
    if (level == 14) {
      return "Senior"
    }
    if (level == 15) {
      return "Principal"
    }
    if (level == -1) {
      return ""
    }
  }

}
