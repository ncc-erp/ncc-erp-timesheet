		
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CapabilitySettingComponent } from './capability-setting.component';
import { CreateEditCapabilitySettingComponent } from './create-edit-capability-setting/create-edit-capability-setting.component';
const routes: Routes = [
    { path: '', component: CapabilitySettingComponent },
    {
        path: 'capability-setting',
        component: CreateEditCapabilitySettingComponent
    },
    
];
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class CapabilitySettingRoutingModule {}