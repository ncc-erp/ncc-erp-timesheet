import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AppAddUserOtherProjectRetroResultComponent } from './app-add-user-other-project-retro-result.component';

describe('AppAddUserOtherProjectRetroResultComponent', () => {
  let component: AppAddUserOtherProjectRetroResultComponent;
  let fixture: ComponentFixture<AppAddUserOtherProjectRetroResultComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AppAddUserOtherProjectRetroResultComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AppAddUserOtherProjectRetroResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
