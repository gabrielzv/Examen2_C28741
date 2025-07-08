<template>
  <div class="vending-machine">
    <header class="machine-header">
      <h1 class="machine-title">Máquina Expendedora</h1>
      <p class="machine-subtitle">Seleccione su refresco favorito</p>
    </header>

    <!-- Cargando -->
    <div v-if="loading" class="loading">
      Cargando productos...
    </div>

    <!-- Error -->
    <div v-else-if="error" class="error">
      {{ error }}
      <br>
      <button @click="loadProducts" style="margin-top: 10px; padding: 5px 15px; background: white; color: #e74c3c; border: none; border-radius: 5px; cursor: pointer;">
        Reintentar
      </button>
    </div>

    <!-- Contenido principal -->
    <div v-else class="machine-content">
      <!-- Lista de productos -->
      <div class="products-section">
        <div class="products-grid">
          <ProductCard 
            v-for="product in products" 
            :key="product.id" 
            :product="product"
            @quantity-changed="onQuantityChanged"
          />
        </div>
      </div>

      <!-- Carrito -->
      <div class="cart-section">
        <CartSummary 
          :cart-items="cartItems"
          :products="products"
          :order-summary="orderSummary"
          :calculating="calculating"
        />
        
        <!-- Panel de pago -->
        <PaymentPanel 
          v-if="orderSummary && orderSummary.total > 0"
          :total-cost="orderSummary.total"
          @payment-success="handlePaymentSuccess"
        />
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, watch } from 'vue'
import ProductCard from './components/ProductCard.vue'
import CartSummary from './components/CartSummary.vue'
import PaymentPanel from './components/PaymentPanel.vue'
import { productService, cartService } from './services/api.js'

export default {
  name: 'App',
  components: {
    ProductCard,
    CartSummary,
    PaymentPanel
  },
  setup() {
    const products = ref([])
    const loading = ref(false)
    const error = ref(null)
    const cartItems = ref([])
    const orderSummary = ref(null)
    const calculating = ref(false)

    const loadProducts = async () => {
      loading.value = true
      error.value = null
      
      try {
        const data = await productService.getAllProducts()
        products.value = data
      } catch (err) {
        error.value = 'Error al cargar los productos. Backend no esta encendido o no responde.'
        console.error('Error loading products:', err)
      } finally {
        loading.value = false
      }
    }

    const onQuantityChanged = (item) => {
      // Actualizar carrito
      const existingItemIndex = cartItems.value.findIndex(
        cartItem => cartItem.productId === item.productId
      )

      if (item.quantity === 0) {
        // Remover del carrito si es 0
        if (existingItemIndex >= 0) {
          cartItems.value.splice(existingItemIndex, 1)
        }
      } else {
        // Agregar o actualizar en el carrito
        if (existingItemIndex >= 0) {
          cartItems.value[existingItemIndex].quantity = item.quantity
        } else {
          cartItems.value.push({
            productId: item.productId,
            quantity: item.quantity
          })
        }
      }
    }

    const calculateTotal = async () => {
      if (cartItems.value.length === 0) {
        orderSummary.value = null
        return
      }

      calculating.value = true
      
      try {
        const cart = {
          items: cartItems.value
        }
        
        const summary = await cartService.calculateTotal(cart)
        orderSummary.value = summary
      } catch (err) {
        console.error('Error calculating total:', err)
        orderSummary.value = {
          errors: ['Error al calcular el total']
        }
      } finally {
        calculating.value = false
      }
    }

    const handlePaymentSuccess = async (paymentResult) => {
      // Limpiar carrito
      cartItems.value = []
      orderSummary.value = null
      
      // Actualizar stock
      await loadProducts()
      
      // Mostrar mensaje del cambio
      alert(`Compra exitosa ${paymentResult.changeAmount > 0 ? `Su cambio es de ₡${paymentResult.changeAmount.toLocaleString()}` : ''}`)
    }

    // Recalcular cuando cambie el carrito
    watch(cartItems, calculateTotal, { deep: true })

    onMounted(() => {
      loadProducts()
    })

    return {
      products,
      loading,
      error,
      cartItems,
      orderSummary,
      calculating,
      loadProducts,
      onQuantityChanged,
      handlePaymentSuccess
    }
  }
}
</script>
