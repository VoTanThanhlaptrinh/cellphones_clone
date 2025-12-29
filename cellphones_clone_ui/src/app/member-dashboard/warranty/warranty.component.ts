import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-warranty',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './warranty.component.html',
    styleUrls: ['./warranty.component.scss']
})
export class WarrantyComponent {
    activeTab = signal<string>('all');

    tabs = [
        { id: 'all', label: 'Tất cả' },
        { id: 'received', label: 'Đã tiếp nhận' },
        { id: 'coordinating', label: 'Đang điều phối' },
        { id: 'repairing', label: 'Đang Sửa' },
        { id: 'repaired', label: 'Đã sửa xong' },
        { id: 'returned', label: 'Đã trả máy' }
    ];

    setActiveTab(tabId: string) {
        this.activeTab.set(tabId);
    }
}
