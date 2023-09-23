using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Ncc.Authorization.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.BackgroundJob
{
    public class WorkingTimeBackgroundJob : BackgroundJob<WorkingTimeBackgroundJobArgs>, ITransientDependency
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<AbsenceDayDetail, long> _absenceDayDetail;

        public WorkingTimeBackgroundJob(IRepository<User, long> userRepository, IRepository<AbsenceDayDetail, long> absenceDayDetail)
        {
            _userRepository = userRepository;
            _absenceDayDetail = absenceDayDetail;
        }
        
        [UnitOfWork]
        public override void Execute(WorkingTimeBackgroundJobArgs args)
        {
            Logger.Info("WorkingTimeBackgroundJob.Execute() started " + JsonConvert.SerializeObject(args));            
            ChangeWorkingTime(args.Target);
            Logger.Info("WorkingTimeBackgroundJob.Execute() end");
        }

        private void ChangeWorkingTime(HistoryWorkingTime workingTime)
        {
            var user = _userRepository.Get(workingTime.UserId) ;                

            user.MorningStartAt = workingTime.MorningStartTime;
            user.MorningEndAt = workingTime.MorningEndTime;
            user.MorningWorking = workingTime.MorningWorkingTime;
            user.AfternoonStartAt = workingTime.AfternoonStartTime;
            user.AfternoonEndAt = workingTime.AfternoonEndTime;
            user.AfternoonWorking = workingTime.AfternoonWorkingTime;
            user.isWorkingTimeDefault = false;

             _userRepository.Update(user);

            ChangeRequestOffOfUserAfterApplydate(user.Id, workingTime.ApplyDate, workingTime.MorningWorkingTime, workingTime.AfternoonWorkingTime);
        }
        private void ChangeRequestOffOfUserAfterApplydate(long userId, DateTime applyDate, double morningWokingTime, double afternoonWokingTime)
        { 
            var absenceDayDetails = _absenceDayDetail.GetAll()
                                                    .Where(s => s.Request.UserId == userId)
                                                    .Where(s => s.DateAt.Date >= applyDate.Date)
                                                    .Where(s => s.Request.Type == RequestType.Off)
                                                    .Where(s => s.DateType == DayType.Morning || s.DateType == DayType.Afternoon)
                                                    .ToList();

            foreach(var item in absenceDayDetails)
            {
                if (item.DateType == DayType.Morning)
                    item.Hour = morningWokingTime;
                else
                    item.Hour = afternoonWokingTime;
            }

            CurrentUnitOfWork.SaveChanges();
        }
    }
}
