export function convertDateToString(string) {
    const time = new Date(string);
    const day = time.getDate();
    const month = time.getMonth() + 1;
    const year = time.getFullYear();
    if (time === null) {
        return null;
    } else {
        var ngay = day + '';
        var thang = month + '';
        if (day < 10) {
            ngay = "0" + day;
        }
        if (month < 10) {
            thang = "0" + month;
        }
        return ngay + "/" + thang + "/" + year;
    }
}

export function pathValueTime(string) {
    const time = new Date(string);
    const day = time.getDate();
    const month = time.getMonth() + 1;
    const year = time.getFullYear();
    const result = {
        date: {
            year: year,
            month: month,
            day: day
        }
    };
    return result;
}

export function countDayInWeek(sDate) {
    const startDate = new Date(sDate);
    const oneDay = 1000 * 60 * 60 * 24;
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var dates = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
    if (startDate === null) {
        console.log('startDate is null');
        return null;
    }

    var rDay = dates.map((value, index) => {
        let nextDay = new Date(startDate.getTime() + oneDay * index);
        return {
            date: value,
            day: nextDay.getDate(),
            month: monthNames[nextDay.getMonth()],
            currentDate: nextDay
        };
    })
    return rDay;
}



export function convertMinuteToHour(minute: number) {
    if (!minute) {
        return '';
    }
    if (typeof minute != 'number') {
        return minute;
    }
    let hour = Math.floor(minute / 60);
    let m = minute % 60;
    let shour = hour < 10 ? `0${hour}` : `${hour}`;
    let sm = m < 10 ? `0${m}` : `${m}`;
    return `${shour}:${sm}`;
}

export function convertHourtoMinute(hourMinute: string) {
    if (!hourMinute) 
        return 0;
    if (hourMinute.indexOf(".") >= 0){
        return convertFloatHourToMinute(hourMinute);
    }

    try {
        let arr = hourMinute.split(':');
        let h = arr[0] ? parseInt(arr[0]) : 0;
        let m = arr[1] ? parseInt(arr[1]) : 0;
        return h * 60 + m;

    } catch{

    }
    return 0;

}

export function convertFloatHourToMinute(floatHour: string) {
    try {
        return parseFloat(floatHour) * 60;
    } catch{

    }
    return 0;

}

export function convertMinuteToFloat(IntHour: number) {
    try {
        return IntHour / 60;
    } catch {

    }
    return 0;
}


