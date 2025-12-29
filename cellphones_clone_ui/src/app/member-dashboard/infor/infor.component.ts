import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-infor',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './infor.component.html',
    styleUrls: ['./infor.component.scss']
})
export class InforComponent {
    userInfo = signal({
        name: 'Võ Tấn Thành',
        phone: '0796692184',
        gender: '-',
        email: 'votanthanh32004@gmail.com',
        dob: '10/01/2004',
        address: '-'
    });
}
