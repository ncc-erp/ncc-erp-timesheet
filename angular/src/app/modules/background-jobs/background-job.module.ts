import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { NgxPaginationModule } from "ngx-pagination";
import { BackgroundJobRoutingModule } from "./background-job-routing.module";
import { BackgroundJobsComponent } from "./background-jobs.component";

@NgModule({
    declarations: [BackgroundJobsComponent],
    imports: [
        CommonModule,
        BackgroundJobRoutingModule,
        SharedModule,
        FormsModule,
        NgxPaginationModule,
    ]
})
export class BackgroundJobModule{

}