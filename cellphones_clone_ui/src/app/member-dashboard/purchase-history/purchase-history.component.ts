import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-purchase-history',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './purchase-history.component.html',
    styleUrls: ['./purchase-history.component.scss']
})
export class PurchaseHistoryComponent {
    activeTab = signal<string>('all');

    tabs = [
        { id: 'all', label: 'Tất cả' },
        { id: 'pending', label: 'Chờ xác nhận' },
        { id: 'confirmed', label: 'Đã xác nhận' },
        { id: 'shipping', label: 'Đang vận chuyển' },
        { id: 'delivered', label: 'Đã giao hàng' },
        { id: 'cancelled', label: 'Đã huỷ' }
    ];

    setActiveTab(tabId: string) {
        this.activeTab.set(tabId);
    }
}
