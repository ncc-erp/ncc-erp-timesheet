import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { RemoteBlacklistComponent } from "./remote-blacklist.component";
const routes:Routes= [
    {
        path: "",
        component: RemoteBlacklistComponent,
    }
]
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class RemoteBlacklistRoutingModule{
}