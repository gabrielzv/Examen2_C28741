<template>
  <div class="payment-panel">
    <div class="payment-header">
      <h3 class="payment-title">Cajero</h3>
      <p class="payment-subtitle">Inserte su pago</p>
    </div>
    
    <!-- Cantidades disponibles -->
    <div class="money-quantities">
      <h4>Billetes y monedas aceptadas:</h4>
      <div class="quantity-grid">
        <div 
          v-for="quantity in quantities" 
          :key="quantity.value"
          class="quantity-item"
        >
          <div class="quantity-info">
            <span class="quantity-icon">{{ quantity.icon }}</span>
            <div class="quantity-details">
              <span class="quantity-name">{{ quantity.name }}</span>
              <span class="quantity-value">₡{{ quantity.value.toLocaleString() }}</span>
            </div>
          </div>
          <div class="quantity-controls">
            <button 
              @click="removeMoney(quantity.type)"
              :disabled="insertedMoney[quantity.type] === 0"
              class="btn-control btn-remove"
              title="Quitar una unidad"
            >
              -
            </button>
            <span class="quantity">{{ insertedMoney[quantity.type] || 0 }}</span>
            <button 
              @click="addMoney(quantity.type, quantity.value)"
              class="btn-control btn-add"
              title="Agregar una unidad"
            >
              +
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Total insertado -->
    <div class="total-section">
      <div class="total-inserted">
        <h4>Total insertado: {{ formatMoney(totalInserted) }}</h4>
        <div v-if="totalInserted > 0" class="inserted-breakdown">
          <div v-for="(quantity, type) in insertedMoney" :key="type">
            <span v-if="quantity > 0" class="breakdown-item">
              {{ getQuantityName(type) }}: {{ quantity }} {{ quantity === 1 ? 'unidad' : 'unidades' }}
            </span>
          </div>
        </div>
      </div>

      <!-- Info del pago -->
      <div v-if="totalCost > 0" class="payment-info">
        <div class="payment-row">
          <span>Costo total:</span>
          <span class="amount cost">{{ formatMoney(totalCost) }}</span>
        </div>
        <div class="payment-row">
          <span>Total insertado:</span>
          <span class="amount inserted">{{ formatMoney(totalInserted) }}</span>
        </div>
        <div class="payment-row change-row" v-if="changeAmount !== null">
          <span>{{ changeAmount >= 0 ? 'Cambio:' : 'Falta:' }}</span>
          <span class="amount" :class="{ 'positive': changeAmount >= 0, 'negative': changeAmount < 0 }">
            {{ formatMoney(Math.abs(changeAmount)) }}
          </span>
        </div>
      </div>
    </div>

    <!-- Botones -->
    <div class="payment-actions">
      <button 
        @click="processPayment"
        :disabled="!canProcessPayment || processing"
        class="btn btn-success"
      >
        <span>Procesar Pago</span>
      </button>
    </div>

    <!-- Resultado del pago -->
    <div v-if="paymentResult" class="payment-result" :class="paymentResult.isSuccessful ? 'success' : 'error'">
      
      <div v-if="paymentResult.isSuccessful && paymentResult.changeAmount > 0" class="change-details">
        <h5>Su cambio ({{ formatMoney(paymentResult.changeAmount) }}):</h5>
        <div class="change-breakdown">
          <div v-for="(quantity, coinType) in paymentResult.changeBreakdown" :key="coinType">
            <span v-if="quantity > 0" class="change-item">
              {{ getQuantityName(coinType) }}: {{ quantity }} {{ quantity === 1 ? 'unidad' : 'unidades' }}
            </span>
          </div>
        </div>
      </div>

      <div v-if="!paymentResult.isSuccessful && paymentResult.errors.length > 0" class="error-details">
        <ul>
          <li v-for="error in paymentResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
    </div>

    <!-- Estado de la maquina -->
    <div v-if="showCashRegister && cashRegisterStatus.length > 0" class="cash-register-status">
      <div class="cash-register-header" @click="toggleCashRegister">
        <h4>Cambio en la máquina</h4>
        <span class="toggle-icon">{{ cashRegisterExpanded ? '▼' : '▶' }}</span>
      </div>
      <div v-if="cashRegisterExpanded" class="money-status-grid">
        <div v-for="slot in filteredCashRegister" :key="slot.coinType" class="money-slot">
          <div class="slot-info">
            <span class="coin-type">{{ getCoinTypeName(slot.coinType) }}</span>
            <span class="coin-value">{{ formatMoney(slot.value) }}</span>
          </div>
          <span class="coin-quantity" :class="getQuantityClass(slot.quantity)">
            {{ slot.quantity }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { paymentService } from '../services/api'

export default {
  name: 'PaymentPanel',
  props: {
    totalCost: {
      type: Number,
      default: 0
    },
    showCashRegister: {
      type: Boolean,
      default: true
    }
  },
  emits: ['payment-success'],
  setup(props, { emit }) {
    const insertedMoney = ref({
      Coin25: 0,
      Coin50: 0,
      Coin100: 0,
      Coin500: 0,
      Bill1000: 0
    })

    const quantities = [
      { type: 'Bill1000', value: 1000, name: 'Billete' },
      { type: 'Coin500', value: 500, name: 'Moneda' },
      { type: 'Coin100', value: 100, name: 'Moneda' },
      { type: 'Coin50', value: 50, name: 'Moneda' },
      { type: 'Coin25', value: 25, name: 'Moneda' }
    ]

    const paymentResult = ref(null)
    const cashRegisterStatus = ref([])
    const cashRegisterExpanded = ref(false)
    const processing = ref(false)

    const totalInserted = computed(() => {
        return Object.entries(insertedMoney.value).reduce((total, [type, quantity]) => {
            const denomination = quantities.find(d => d.type === type)
            return total + (denomination ? denomination.value * quantity : 0)
        }, 0)
    })


    const changeAmount = computed(() => {
      if (props.totalCost === 0) return null
      return totalInserted.value - props.totalCost
    })

    const canProcessPayment = computed(() => {
      return totalInserted.value > 0 && props.totalCost > 0 && totalInserted.value >= props.totalCost
    })

    const filteredCashRegister = computed(() => {
      return cashRegisterStatus.value.filter(slot => slot.coinType !== 'Bill1000')
    })

    const addMoney = (type, value) => {
      insertedMoney.value[type]++
      paymentResult.value = null
    }

    const removeMoney = (type) => {
      if (insertedMoney.value[type] > 0) {
        insertedMoney.value[type]--
        paymentResult.value = null
      }
    }

    const clearPayment = () => {
      Object.keys(insertedMoney.value).forEach(key => {
        insertedMoney.value[key] = 0
      })
      paymentResult.value = null
    }

    const formatMoney = (amount) => {
      return `₡${amount.toLocaleString()}`
    }

    const getQuantityName = (type) => {
      const quantity = quantities.find(d => d.type === type)
      if (!quantity) return type
      return `${quantity.name} ${formatMoney(quantity.value)}`
    }

    const getCoinTypeName = (coinType) => {
      const typeMap = {
        'Coin25': 'Moneda ₡25',
        'Coin50': 'Moneda ₡50',
        'Coin100': 'Moneda ₡100',
        'Coin500': 'Moneda ₡500',
        'Bill1000': 'Billete ₡1000'
      }
      return typeMap[coinType] || coinType
    }

    const getQuantityClass = (quantity) => {
      if (quantity === 0) return 'empty'
      if (quantity <= 5) return 'low'
      return 'normal'
    }

    const toggleCashRegister = () => {
      cashRegisterExpanded.value = !cashRegisterExpanded.value
    }

    const processPayment = async () => {
      processing.value = true
      try {
        const payment = {
          insertedMoney: insertedMoney.value,
          totalInserted: totalInserted.value
        }
        
        const result = await paymentService.processPayment(payment, props.totalCost)
        paymentResult.value = result
        
        if (result.isSuccessful) {
          emit('payment-success', result)
          clearPayment()
          await loadCashRegisterStatus()
        }
      } catch (error) {
        console.error('Error processing payment:', error)
        paymentResult.value = {
          isSuccessful: false,
          message: 'Error al procesar el pago. Intente nuevamente.',
          errors: ['Error de conexión con el servidor']
        }
      } finally {
        processing.value = false
      }
    }

    const loadCashRegisterStatus = async () => {
      try {
        const status = await paymentService.getCashRegisterStatus()
        cashRegisterStatus.value = status
      } catch (error) {
        console.error('Error loading cash register status:', error)
      }
    }

    onMounted(() => {
      loadCashRegisterStatus()
    })

    return {
      insertedMoney,
      quantities,
      paymentResult,
      cashRegisterStatus,
      cashRegisterExpanded,
      processing,
      totalInserted,
      changeAmount,
      canProcessPayment,
      filteredCashRegister,
      addMoney,
      removeMoney,
      clearPayment,
      formatMoney,
      getQuantityName,
      getCoinTypeName,
      getQuantityClass,
      toggleCashRegister,
      processPayment
    }
  }
}
</script>

<style scoped>
.payment-panel {
  max-width: 600px;
  margin: 0 auto;
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background-color: #f9f9f9;
}

.payment-header {
  text-align: center;
  margin-bottom: 20px;
}

.payment-title {
  margin: 0;
  color: #333;
}

.payment-subtitle {
  margin: 5px 0 0 0;
  color: #666;
}

.money-quantities h4 {
  margin-bottom: 15px;
  color: #333;
}

.quantity-grid {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.quantity-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px;
  background-color: white;
  border: 1px solid #ddd;
  border-radius: 6px;
}

.quantity-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.quantity-icon {
  font-size: 24px;
}

.quantity-details {
  display: flex;
  flex-direction: column;
}

.quantity-name {
  font-weight: bold;
  color: #333;
}

.quantity-value {
  color: #666;
  font-size: 14px;
}

.quantity-controls {
  display: flex;
  align-items: center;
  gap: 10px;
}

.btn-control {
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 4px;
  font-size: 18px;
  font-weight: bold;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-add {
  background-color: #28a745;
  color: white;
}

.btn-add:hover:not(:disabled) {
  background-color: #218838;
}

.btn-remove {
  background-color: #dc3545;
  color: white;
}

.btn-remove:hover:not(:disabled) {
  background-color: #c82333;
}

.btn-control:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.quantity {
  min-width: 30px;
  text-align: center;
  font-weight: bold;
  font-size: 16px;
}

.total-section {
  margin: 20px 0;
  padding: 15px;
  background-color: white;
  border-radius: 6px;
  border: 1px solid #ddd;
}

.total-inserted h4 {
  margin: 0 0 10px 0;
  color: #333;
}

.inserted-breakdown {
  margin-top: 10px;
}

.breakdown-item {
  display: block;
  color: #666;
  font-size: 14px;
  margin-bottom: 5px;
}

.payment-info {
  margin-top: 15px;
  padding-top: 15px;
  border-top: 1px solid #eee;
}

.payment-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.amount {
  font-weight: bold;
}

.amount.cost {
  color: #dc3545;
}

.amount.inserted {
  color: #007bff;
}

.amount.positive {
  color: #28a745;
}

.amount.negative {
  color: #dc3545;
}

.change-row {
  border-top: 1px solid #eee;
  padding-top: 8px;
  margin-top: 8px;
}

.payment-actions {
  display: flex;
  gap: 10px;
  margin: 20px 0;
}

.btn {
  flex: 1;
  padding: 12px 20px;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #545b62;
}

.btn-success {
  background-color: #28a745;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background-color: #218838;
}

.payment-result {
  padding: 15px;
  border-radius: 6px;
  margin: 20px 0;
}

.payment-result.success {
  background-color: #d4edda;
  border: 1px solid #c3e6cb;
  color: #155724;
}

.payment-result.error {
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  color: #721c24;
}

.change-details {
  margin-top: 15px;
}

.change-breakdown {
  margin-top: 10px;
}

.change-item {
  display: block;
  margin-bottom: 5px;
  font-size: 14px;
}

.error-details ul {
  margin: 10px 0 0 20px;
}

.cash-register-status {
  margin-top: 20px;
  border: 1px solid #ddd;
  border-radius: 6px;
  background-color: white;
}

.cash-register-header {
  padding: 15px;
  background-color: #f8f9fa;
  border-bottom: 1px solid #ddd;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.cash-register-header h4 {
  margin: 0;
}

.toggle-icon {
  font-size: 14px;
  color: #666;
}

.money-status-grid {
  padding: 15px;
}

.money-slot {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid #eee;
}

.money-slot:last-child {
  border-bottom: none;
}

.slot-info {
  display: flex;
  flex-direction: column;
}

.coin-type {
  font-weight: bold;
  color: #333;
}

.coin-value {
  color: #666;
  font-size: 14px;
}

.coin-quantity {
  font-weight: bold;
  padding: 4px 8px;
  border-radius: 4px;
}

.coin-quantity.empty {
  background-color: #f8d7da;
  color: #721c24;
}

.coin-quantity.low {
  background-color: #fff3cd;
  color: #856404;
}

.coin-quantity.normal {
  background-color: #d4edda;
  color: #155724;
}
</style>
