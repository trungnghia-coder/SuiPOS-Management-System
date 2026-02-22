// ============================================
// REPORTS PAGE - DASHBOARD & CHARTS
// ============================================

let salesChart = null;

// ========== INIT ==========
document.addEventListener('DOMContentLoaded', function() {
    // Set default date range (last 30 days)
    const today = new Date();
    const last30Days = new Date();
    last30Days.setDate(today.getDate() - 30);
    
    document.getElementById('fromDate').value = last30Days.toISOString().split('T')[0];
    document.getElementById('toDate').value = today.toISOString().split('T')[0];
    
    // Load initial report
    loadReport();
});

// ========== LOAD REPORT DATA ==========
async function loadReport() {
    try {
        const fromDate = document.getElementById('fromDate').value;
        const toDate = document.getElementById('toDate').value;
        const groupBy = document.getElementById('groupBy').value;
        
        const url = `/Reports/GetDashboardData?fromDate=${fromDate}&toDate=${toDate}&groupBy=${groupBy}`;
        const response = await fetch(url);
        const data = await response.json();
        
        updateStats(data);
        updateChart(data);
        updateTopProducts(data.topSellingProducts);
        
    } catch (error) {
        console.error('Error loading report:', error);
        alert('Lỗi khi tải báo cáo');
    }
}

// ========== UPDATE STATS CARDS ==========
function updateStats(data) {
    document.getElementById('totalRevenue').textContent = formatNumber(data.totalRevenue);
    document.getElementById('totalReceived').textContent = formatNumber(data.totalReceived);
    document.getElementById('actualReceived').textContent = formatNumber(data.actualReceived);
    document.getElementById('totalOrders').textContent = data.totalOrders.toLocaleString();
    document.getElementById('totalProducts').textContent = data.totalProducts.toLocaleString();
}

// ========== UPDATE CHART ==========
function updateChart(data) {
    const labels = data.revenueByDate.map(r => {
        const date = new Date(r.date);
        const groupBy = document.getElementById('groupBy').value;
        
        if (groupBy === 'month') {
            return date.toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit' });
        } else if (groupBy === 'year') {
            return date.toLocaleDateString('vi-VN', { year: 'numeric' });
        } else {
            return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
        }
    });
    
    const revenueData = data.revenueByDate.map(r => r.revenue);
    const orderData = data.orderCountByDate.map(o => o.orderCount);
    
    // Destroy existing chart
    if (salesChart) salesChart.destroy();
    
    // Create combined chart
    const ctx = document.getElementById('salesChart').getContext('2d');
    salesChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    type: 'line',
                    label: 'Doanh thu thuần',
                    data: revenueData,
                    borderColor: 'rgb(59, 130, 246)',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 2,
                    pointRadius: 6,
                    pointBackgroundColor: 'rgb(59, 130, 246)',
                    pointHoverRadius: 8,
                    yAxisID: 'y',
                    tension: 0.4,
                    order: 1
                },
                {
                    type: 'bar',
                    label: 'Đơn hàng',
                    data: orderData,
                    backgroundColor: 'rgb(249, 115, 22)',
                    yAxisID: 'y1',
                    order: 2,
                    barPercentage: 0.6
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: true,
                    position: 'nearest',
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: 'rgba(59, 130, 246, 0.5)',
                    borderWidth: 1,
                    padding: 12,
                    displayColors: true,
                    callbacks: {
                        beforeBody: function(context) {
                            const chart = context[0].chart;
                            const dataIndex = context[0].dataIndex;
                            const totalPoints = chart.data.labels.length;
                            
                            if (dataIndex > totalPoints * 0.6) {
                                this.options.xAlign = 'right';
                            } else {
                                this.options.xAlign = 'left';
                            }
                            
                            this.options.yAlign = 'center';
                        },
                        label: function(context) {
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed.y !== null) {
                                if (context.datasetIndex === 0) {
                                    label += formatNumber(context.parsed.y);
                                } else {
                                    label += context.parsed.y;
                                }
                            }
                            return label;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    }
                },
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Tổng tiền'
                    },
                    ticks: {
                        callback: function(value) {
                            return formatNumber(value);
                        }
                    }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    title: {
                        display: true,
                        text: 'Số lượng'
                    },
                    grid: {
                        drawOnChartArea: false,
                    }
                }
            }
        }
    });
}

// ========== UPDATE TOP PRODUCTS ==========
function updateTopProducts(products) {
    const container = document.getElementById('topProductsList');
    
    if (!products || products.length === 0) {
        container.innerHTML = '<div class="text-center py-8 text-gray-500">Không có dữ liệu</div>';
        return;
    }
    
    container.innerHTML = products.map(p => `
        <div class="flex gap-3 p-3 bg-gray-50 rounded-lg">
            <img src="${p.imageUrl || 'https://placehold.co/60x60/webp?text=No+Image'}" 
                 alt="${p.productName}" 
                 class="w-14 h-14 object-cover rounded">
            <div class="flex-1">
                <div class="text-sm font-medium text-gray-900 line-clamp-2">${p.productName}</div>
                <div class="text-xs text-gray-500 mt-1">Đã bán được ${p.totalSold} sản phẩm</div>
                <div class="text-sm font-semibold text-blue-600 mt-1">Doanh thu thuần: ${formatNumber(p.totalRevenue)}</div>
            </div>
        </div>
    `).join('');
}

// ========== HELPERS ==========
function formatNumber(value) {
    return new Intl.NumberFormat('vi-VN').format(value);
}

