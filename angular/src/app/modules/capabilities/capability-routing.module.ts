import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CapabilityComponent } from './capability.component';
const routes: Routes = [
    {path: '', component: CapabilityComponent}
];
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class CapabilityRoutingModule {}