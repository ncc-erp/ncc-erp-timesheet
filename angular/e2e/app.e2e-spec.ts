import { TimesheetTemplatePage } from './app.po';

describe('Timesheet App', function() {
  let page: TimesheetTemplatePage;

  beforeEach(() => {
    page = new TimesheetTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
