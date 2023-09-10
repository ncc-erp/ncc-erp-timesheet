import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { BackgroundJobsComponent } from "./background-jobs.component";
const routes:Routes= [
    {
        path: "",
        component: BackgroundJobsComponent,
    }
]
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class BackgroundJobRoutingModule{

}