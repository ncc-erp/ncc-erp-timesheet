import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MainRoutingModule } from './main-routing.module';
import { MainComponent } from './main.component';
import { InternsInfoModule } from './report/interns-info/interns-info.module';
import {AbsenceModule} from '@app/modules/report/absence/absence.module';
import { ReviewComponent } from './review/review.component';

@NgModule({
    declarations: [MainComponent],
    imports: [
        CommonModule,
        MainRoutingModule
    ]
})
export class MainModule {
}
