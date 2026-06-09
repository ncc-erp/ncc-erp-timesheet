import { Component, Injector, OnInit } from '@angular/core';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';

@Component({
  selector: 'app-violation-check',
  templateUrl: './violation-check.component.html',
  styleUrls: ['./violation-check.component.css']
})
export class ViolationCheckComponent extends AppComponentBase implements OnInit {
  selectedDate: Date;
  isLoading = false;
  hasChecked = false;
  twoOrLessDaysViolations: any[] = [];
  moreThanTwoDaysViolations: any[] = [];

  constructor(
    injector: Injector,
    private absenceRequestService: AbsenceRequestService,
  ) {
    super(injector);
  }

  ngOnInit() {}

  onDateChange(event: any) {
    if (!event.value) return;
    this.selectedDate = event.value;
    this.checkViolations();
  }

  checkViolations() {
    if (!this.selectedDate) return;
    const dateStr = moment(this.selectedDate).format('YYYY-MM-DD');
    this.isLoading = true;
    this.hasChecked = false;
    this.twoOrLessDaysViolations = [];
    this.moreThanTwoDaysViolations = [];

    this.absenceRequestService.getRequestOffViolationByDate(dateStr).subscribe(
      (res: any) => {
        this.twoOrLessDaysViolations = (res.result && res.result.twoOrLessDaysViolations) || [];
        this.moreThanTwoDaysViolations = (res.result && res.result.moreThanTwoDaysViolations) || [];
        this.hasChecked = true;
        this.isLoading = false;
      },
      () => {
        this.isLoading = false;
      }
    );
  }
}
